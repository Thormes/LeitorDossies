Imports HtmlAgilityPack

Namespace Minerador

    Public Class DossiePrevidenciario
        Public Property Sucesso As Boolean
        Public Property Mensagem As String
        Public Property TipoDeDossie As TipoDossie
        Public Property Autor As Pessoa
        ''' <summary>
        ''' Cria o dossi�, necess�rio informar no m�nimo o conte�do html do dossi� (c�digo-fonte da p�gina)
        ''' </summary>
        ''' <param name="html"></param>
        Public Sub New(html As String)
            Sucesso = False
            TipoDeDossie = 0
            Autor = New Pessoa
            LerDossie(html)
        End Sub
        ''' <summary>
        ''' Sobrecarga pra realizar a leitura do dossi� j� informando uma leitura anterior, para acr�scimo e atualiza��o de informa��es
        ''' </summary>
        ''' <param name="html"></param>
        ''' <param name="Pessoa"></param>
        Public Sub New(html As String, Pessoa As Pessoa)
            Sucesso = False
            TipoDeDossie = 0
            Autor = Pessoa
            LerDossie(html, Autor)
        End Sub
        ''' <summary>
        ''' Realiza a leitura do html para identificar se � dossi� previdenci�rio ou m�dico e realiza o preenchimento dos dados do dossi� com os dados do segurado
        ''' Se fornecido uma pessoa, realizar adi��o dos dados, se n�o cria um novo dossi�. Ideal realizar primeiro leitura do dossi� previdenci�rio e posteriormente dossi� m�dico, para complementar os dados do benef�cio com os laudos encontrados
        ''' </summary>
        ''' <param name="html"></param>
        ''' <param name="Pessoa"></param>
        Friend Sub LerDossie(html As String, Optional Pessoa As Pessoa = Nothing)
            If Pessoa Is Nothing Then Pessoa = New Pessoa
            html = EliminarQuebras(html)
            html = NormalizarEspacos(html)

            If html.Contains("<title>SAPIENS - Dossi� Previd�nci�rio</title>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Previdenci�rio
                Mensagem = "Dossi� Previdenci�rio"
            ElseIf html.Contains("<b>DOSSI� M�DICO</b>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.M�dico
                Mensagem = "Dossi� M�dico"
            Else
                Sucesso = False
                TipoDeDossie = TipoDossie.Inv�lido
                Mensagem = "Dossi� n�o reconhecido"
            End If
            If Sucesso = True Then
                Try
                    Dim textoHTML As New HtmlDocument()
                    textoHTML.LoadHtml(html)
                    Dim tabelas As HtmlNodeCollection = textoHTML.DocumentNode.SelectNodes("//table")
                    Dim listaTabelas As New List(Of Integer)
                    For Each tabela In tabelas
                        Dim teste As ParseTabelaResult
                        teste = parseTabelas.IdentificarTabela(tabela)
                        If teste.IdTabela > 0 And Not listaTabelas.Contains(teste.IdTabela) Then
                            ParseTabela(teste.IdTabela, teste.Elemento, Autor)
                            listaTabelas.Add(teste.IdTabela)
                        End If
                    Next
                    CalculaTempoContribuicao()
                    ProcessaBeneficios()
                Catch ex As Exception
                    Sucesso = False
                    Mensagem &= Environment.NewLine & "Erro ao realizar a leitura do Dossi�: " & ex.StackTrace
                End Try
            End If

        End Sub
        ''' <summary>
        ''' Calcula o tempo de contribui��o do segurado, utilizando como par�metro os v�nculos empregat�cios detectados nos dossi�s
        ''' A contagem leva em considera��o ano de 360 dias e m�s de 30 dias. Per�odo concomitante � computado apenas uma �nica vez
        ''' </summary>
        Friend Sub CalculaTempoContribuicao()
            Dim TotalDeDias As Integer
            For Each vinc In Autor.V�nculos
                If vinc.Fim <> "" OrElse vinc.�ltimaRemunera��o <> "" Then 'Se tem data final ou �ltima remunera��o
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.�ltimaRemunera��o)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If

                    For Each vinc2 In Autor.V�nculos 'An�lise se o v�nculo � concomitante com outros, para fins de contagem do tempo de servi�o
                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se n�o � o mesmo v�nculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.�ltimaRemunera��o)
                                DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                            Else
                                DataFinal2 = CDate(vinc2.Fim)
                            End If
                            If DataInicial >= DataInicial2 AndAlso DataInicial < DataFinal2 Then
                                If DataFinal <= DataFinal2 Then
                                    DataInicial = DataFinal
                                Else
                                    DataInicial = DataFinal2
                                End If
                            End If
                            If DataFinal <= DataFinal2 AndAlso DataFinal > DataInicial2 Then
                                If DataInicial >= DataInicial2 Then
                                    DataFinal = DataInicial
                                Else
                                    DataFinal = DataInicial2
                                End If
                            End If
                        End If
                    Next

                    'Dim Dias As Integer = DateDiff(DateInterval.Day, CDate(vinc.Inicio), CDate(vinc.Fim)) - 1
                    Dim Dias As Integer = Dias360(DataInicial, DataFinal, True) + 1
                    Dim Anos As Integer = Math.Floor(Dias / 360)
                    Dim Meses As Integer = Math.Floor((Dias Mod 360) / 30)
                    Dim DiasRestantes As Integer = (Dias Mod 360) Mod 30
                    vinc.Anos = Anos
                    vinc.Dias = DiasRestantes
                    vinc.Meses = Meses
                    vinc.DiasTotais = Dias
                    TotalDeDias += Dias
                End If
            Next
            Autor.TotalDias = TotalDeDias
            Dim AnosTotais As Integer = Math.Floor(TotalDeDias / 360)
            Dim MesesTotais As Integer = Math.Floor((TotalDeDias Mod 360) / 30)
            Dim DiasTotaisRestantes As Integer = (TotalDeDias Mod 360) Mod 30
            Autor.TempoContribui��o = String.Format("{0} Anos, {1} Meses e {2} Dias", AnosTotais, MesesTotais, DiasTotaisRestantes)

        End Sub
        ''' <summary>
        ''' Para facilita��o do acesso aos dados, escolhe o �ltimo benef�cio indeferido, o �ltimo benef�cio cessado e a lista de benef�cios ativos do segurado
        ''' </summary>
        Friend Sub ProcessaBeneficios()
            Dim benefCessado As New Benef�cio
            Dim benefIndeferido As New Benef�cio
            Dim benefAtivo As New List(Of Benef�cio)
            For Each benef In Autor.Benef�cios
                If benef.Status IsNot Nothing Then
                    If benef.Status = "INDEFERIDO" Then

                        If benefIndeferido.DER Is Nothing Then
                            benefIndeferido = benef
                        Else
                            If CDate(benefIndeferido.DER) < CDate(benef.DER) Then benefIndeferido = benef
                        End If
                    ElseIf benef.Status = "CESSADO" Then

                        If benefCessado.DCB Is Nothing Then
                            benefCessado = benef
                        Else
                            If CDate(benefCessado.DCB) < CDate(benef.DCB) Then benefCessado = benef
                        End If
                    ElseIf benef.Status = "ATIVO" Or benef.Status.Contains("RECEBENDO MENSALIDADE") Then
                        benefAtivo.Add(benef)
                    End If
                    benef.CalculaTempoContribuicao(benef.DER, Autor)
                End If
            Next
            Autor.�ltimoBenef�cioCessado = benefCessado
            Autor.�ltimoBenef�cioIndeferido = benefIndeferido
            Autor.Benef�ciosAtivos = benefAtivo
        End Sub
    End Class
    Public Class OutroProcesso
        Public Property Processo As String
        Public Property Assunto As String
        Public Property Interessados As String
        Public Property Org�oJulgador As String
        Public Property Ajuizamento As String
        Public Property DataAbertura As String

    End Class
    Public Class Pessoa
        Public Property Nome As String
        Public Property Nascimento As String
        Public Property NIT As New List(Of String)
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filia��o As String
        Public Property Endere�oPrincipal As String
        Public Property Endere�oSecund�rio As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Benef�cios As New List(Of Benef�cio)
        Public Property V�nculos As New List(Of V�nculo)
        Public Property �ltimoBenef�cioIndeferido As Benef�cio
        Public Property �ltimoBenef�cioCessado As Benef�cio
        Public Property Benef�ciosAtivos As List(Of Benef�cio)
        Public Property TempoContribui��o As String
        Public Property TotalDias As Integer

    End Class
    Public Class Benef�cio
        Public Property Sequencial As Integer
        Public Property NB As String
        Public Property Esp�cie As String
        Public Property DER As String
        Public Property DIB As String
        Public Property DDB As String
        Public Property DCB As String
        Public Property DIP As String
        Public Property Status As String
        Public Property Motivo As String
        Public Property RMI As Double
        Public Property SB As Double
        Public Property Coeficiente As Double
        Public Property RMA As Double
        Public Property DAT As String
        Public Property DataNBAnterior As String
        Public Property Data�bito As String
        Public Property IRT As Double
        Public Property Indice1298 As Double
        Public Property Indice0104 As Double
        Public Property Filia��o As String
        Public Property RamoAtividade As String
        Public Property NaturezaOcupa��o As String
        Public Property TipoConcess�o As String
        Public Property Tratamento As Integer
        Public Property DRD As String
        Public Property APSConcessora As String
        Public Property APSMantenedora As String
        Public Property APSRequerimento As String
        Public Property �bitoInstituidor As String
        Public Property APR As Double
        Public Property TempoAteDER As String
        Public Property CartaConcess�o As CartaConcess�o
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)
        ''' <summary>
        ''' Calcula o tempo de contribui��o do segurado at� a DER do benef�cio. Considerandos apenas os v�nculos n�o concomitantes
        ''' </summary>
        ''' <param name="DER"></param>
        ''' <param name="Autor"></param>
        Friend Sub CalculaTempoContribuicao(DER As Date, Autor As Pessoa)
            Dim TotalDeDias As Integer
            For Each vinc In Autor.V�nculos

                If vinc.Fim <> "" OrElse vinc.�ltimaRemunera��o <> "" Then 'Se tem data final ou �ltima remunera��o
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.�ltimaRemunera��o)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If
                    If DataInicial > DER Then Continue For
                    If DataFinal > DER Then DataFinal = DER

                    For Each vinc2 In Autor.V�nculos 'An�lise se o v�nculo � concomitante com outros, para fins de contagem do tempo de servi�o
                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se n�o � o mesmo v�nculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.�ltimaRemunera��o)
                                DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                            Else
                                DataFinal2 = CDate(vinc2.Fim)
                            End If
                            If DataInicial >= DataInicial2 AndAlso DataInicial < DataFinal2 Then
                                If DataFinal <= DataFinal2 Then
                                    DataInicial = DataFinal
                                Else
                                    DataInicial = DataFinal2
                                End If
                            End If
                            If DataFinal <= DataFinal2 AndAlso DataFinal > DataInicial2 Then
                                If DataInicial >= DataInicial2 Then
                                    DataFinal = DataInicial
                                Else
                                    DataFinal = DataInicial2
                                End If
                            End If
                        End If
                    Next

                    'Dim Dias As Integer = DateDiff(DateInterval.Day, CDate(vinc.Inicio), CDate(vinc.Fim)) - 1
                    Dim Dias As Integer = Dias360(DataInicial, DataFinal, True) + 1
                    Dim Anos As Integer = Math.Floor(Dias / 360)
                    Dim Meses As Integer = Math.Floor((Dias Mod 360) / 30)
                    Dim DiasRestantes As Integer = (Dias Mod 360) Mod 30
                    vinc.Anos = Anos
                    vinc.Dias = DiasRestantes
                    vinc.Meses = Meses
                    vinc.DiasTotais = Dias
                    TotalDeDias += Dias
                End If
            Next

            Dim AnosTotais As Integer = Math.Floor(TotalDeDias / 360)
            Dim MesesTotais As Integer = Math.Floor((TotalDeDias Mod 360) / 30)
            Dim DiasTotaisRestantes As Integer = (TotalDeDias Mod 360) Mod 30
            TempoAteDER = String.Format("{0} Anos, {1} Meses e {2} Dias", AnosTotais, MesesTotais, DiasTotaisRestantes)

        End Sub
    End Class
    Public Class V�nculo
        Public Property Sequencial As Integer
        Public Property NIT As String
        Public Property CNPJ As String
        Public Property Origem As String
        Public Property Inicio As String
        Public Property Fim As String
        Public Property Filia��o As String
        Public Property Ocupa��o As String
        Public Property �ltimaRemunera��o As String
        Public Property Anos As Integer
        Public Property Meses As Integer
        Public Property Dias As Integer
        Public Property DiasTotais As Integer
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remunera��es As New List(Of Remunera��o)
        Public Property Recolhimentos As New List(Of Recolhimento)

    End Class
    Public Class Remunera��o
        Public Property Compet�ncia As String
        Public Property Remunera��o As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class Recolhimento
        Public Property Compet�ncia As String
        Public Property DataPagamento As String
        Public Property Contribui��o As Double
        Public Property Sal�rioContribui��o As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class Indicador
        Public Property Indicador As String
        Public Property Descri��o As String
    End Class
    Public Class CartaConcess�o
        Public Property Despacho As String
        Public Property Resumo As String
        Public Property Sal�rios As New List(Of Sal�rioConcess�o)

    End Class
    Public Class Sal�rioConcess�o
        Public Property Sequencial As Integer
        Public Property Compet�ncia As String
        Public Property Sal�rio As Double
        Public Property �ndice As Double
        Public Property Corrigido As Double
        Public Property Observa��o As String
    End Class
    Public Class HISCRE
        Public Property Compet�ncia As String
        Public Property Inicial As String
        Public Property Final As String
        Public Property Bruto As Double
        Public Property L�quido As Double
        Public Property MR As Double
        Public Property Descontos As Double
        Public Property Meio As String
        Public Property Status As String
        Public Property Previs�o As String
        Public Property Pagamento As String
        Public Property Invalidado As Boolean
        Public Property Isento As Boolean
        Public Property Cr�ditos As New List(Of Cr�dito)
    End Class
    Public Class Cr�dito
        Public Property Rubrica As Integer
        Public Property Descri��o As String
        Public Property Valor As Double
    End Class
    Public Class Laudo
        Public Property Benef�cio As String
        Public Property NB As String
        Public Property Requerimento As String
        Public Property Ocupa��o As String
        Public Property DataExame As String
        Public Property DER As String
        Public Property DIB As String
        Public Property DID As String
        Public Property DII As String
        Public Property DCB As String
        Public Property CID As String
        Public Property Hist�rico As String
        Public Property ExameF�sico As String
        Public Property Considera��es As String
        Public Property Resultado As String
        Public Property EncaminhaReabilita��o As Boolean
        Public Property AcidenteTrabalho As Boolean
        Public Property AuxilioAcidente As String
        Public Property Isen��oCarencia As Boolean
        Public Property Sugest�oLI As Boolean
    End Class
    Public Class ParseTabelaResult
        Property Sucesso As Boolean
        Property IdTabela As TipoTabela
        Property Elemento As HtmlNode
        Public Sub New()
            Sucesso = False
            IdTabela = 0
            Elemento = Nothing
        End Sub
    End Class
    Public Enum TipoTabela
        N�oIdentificada = 0
        FichaSint�tica = 1
        Rela��oProcessos = 2
        Requerimentos = 3
        Rela��esPrevidenci�rias = 4
        Remunera��es = 5
        CartaConcess�o = 6
        HISCRE = 7
        Laudos = 8
    End Enum
    Public Enum TipoDossie
        Inv�lido = 0
        Previdenci�rio = 1
        M�dico = 2
    End Enum
End Namespace