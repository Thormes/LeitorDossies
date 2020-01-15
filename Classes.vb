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
                TipoDeDossie = TipoDossie.Previdenciario
                Mensagem = "Dossi� Previdenci�rio"
            ElseIf html.Contains("<b>DOSSI� M�DICO</b>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Medico
                Mensagem = "Dossi� M�dico"
            Else
                Sucesso = False
                TipoDeDossie = TipoDossie.Invalido
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
            For Each vinc In Autor.Vinculos
                If vinc.Fim <> "" OrElse vinc.UltimaRemuneracao <> "" Then 'Se tem data final ou �ltima remunera��o
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.UltimaRemuneracao)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If

                    Dim InicialAbsorvido As Boolean = False
                    Dim FinalAbsorvido As Boolean = False

                    For Each vinc2 In Autor.Vinculos 'An�lise se o v�nculo � concomitante com outros, para fins de contagem do tempo de servi�o

                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se n�o � o mesmo v�nculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.UltimaRemuneracao)
                                DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                            Else
                                DataFinal2 = CDate(vinc2.Fim)
                            End If
                            If DataInicial >= DataInicial2 AndAlso DataInicial < DataFinal2 Then
                                InicialAbsorvido = True
                                If DataFinal <= DataFinal2 Then
                                    DataInicial = DataFinal
                                Else
                                    DataInicial = DataFinal2
                                End If
                            End If
                            If DataFinal <= DataFinal2 AndAlso DataFinal > DataInicial2 Then
                                FinalAbsorvido = True
                                If DataInicial >= DataInicial2 Then
                                    DataFinal = DataInicial
                                Else
                                    DataFinal = DataInicial2
                                End If
                            End If
                        End If
                    Next

                    'Dim Dias As Integer = DateDiff(DateInterval.Day, CDate(vinc.Inicio), CDate(vinc.Fim)) - 1
                    Dim Dias As Integer
                    If InicialAbsorvido And FinalAbsorvido Then
                        Dias = 0
                    Else
                        Dias = Dias360(DataInicial, DataFinal, True) + 1
                    End If
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
            Autor.TempoContribuicao = String.Format("{0} Anos, {1} Meses e {2} Dias", AnosTotais, MesesTotais, DiasTotaisRestantes)

        End Sub
        ''' <summary>
        ''' Para facilita��o do acesso aos dados, escolhe o �ltimo benef�cio indeferido, o �ltimo benef�cio cessado e a lista de benef�cios ativos do segurado
        ''' </summary>
        Friend Sub ProcessaBeneficios()
            Dim benefCessado As Beneficio
            Dim benefIndeferido As Beneficio
            Dim benefMensalidade As Beneficio
            Dim benefAtivo As New List(Of Beneficio)
            For Each benef In Autor.Beneficios
                If benef.Status IsNot Nothing Then
                    If benef.Status = "INDEFERIDO" Then
                        If benefIndeferido Is Nothing Then
                            benefIndeferido = benef
                        Else
                            If CDate(benefIndeferido.DER) < CDate(benef.DER) Then benefIndeferido = benef
                        End If
                    ElseIf benef.Status = "CESSADO" Then
                        If benefCessado Is Nothing Then
                            benefCessado = benef
                        Else
                            If CDate(benefCessado.DCB) < CDate(benef.DCB) Then benefCessado = benef
                        End If
                    ElseIf benef.Status = "ATIVO" Then
                        benefAtivo.Add(benef)
                    ElseIf benef.Status.Contains("RECEBENDO MENSALIDADE") Then
                        If benefMensalidade Is Nothing Then
                            benefMensalidade = benef
                        Else
                            If CDate(benefMensalidade.DCB) < CDate(benef.DCB) Then benefMensalidade = benef
                        End If
                    End If
                    benef.CalculaTempoContribuicao(benef.DER, Autor)
                End If
            Next
            If benefCessado IsNot Nothing Then Autor.UltimoBeneficioCessado = benefCessado
            If benefIndeferido IsNot Nothing Then Autor.UltimoBeneficioIndeferido = benefIndeferido
            Autor.BeneficiosAtivos = benefAtivo
            If benefMensalidade IsNot Nothing Then Autor.BeneficioEmMensalidadeRecuperacao = benefMensalidade
        End Sub
    End Class
    Public Class OutroProcesso
        Public Property Processo As String
        Public Property Assunto As String
        Public Property Interessados As String
        Public Property OrgaoJulgador As String
        Public Property Ajuizamento As String
        Public Property DataAbertura As String

    End Class
    Public Class Pessoa
        Public Property Nome As String
        Public Property Nascimento As String
        Public Property NIT As New List(Of String)
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filiacao As String
        Public Property Sexo As String
        Public Property EnderecoPrincipal As String
        Public Property Endere�oSecundario As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Beneficios As New List(Of Beneficio)
        Public Property Vinculos As New List(Of Vinculo)
        Public Property UltimoBeneficioIndeferido As Beneficio
        Public Property UltimoBeneficioCessado As Beneficio
        Public Property BeneficioEmMensalidadeRecuperacao As Beneficio
        Public Property BeneficiosAtivos As List(Of Beneficio)
        Public Property TempoContribuicao As String
        Public Property TotalDias As Integer

    End Class
    Public Class Beneficio
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
        Public Property DataObito As String
        Public Property IRT As Double
        Public Property Indice1298 As Double
        Public Property Indice0104 As Double
        Public Property Filiacao As String
        Public Property RamoAtividade As String
        Public Property NaturezaOcupacao As String
        Public Property TipoConcessao As String
        Public Property Tratamento As Integer
        Public Property DRD As String
        Public Property APSConcessora As String
        Public Property APSMantenedora As String
        Public Property APSRequerimento As String
        Public Property ObitoInstituidor As String
        Public Property APR As Double
        Public Property TempoAteDER As String
        Public Property CartaConcessao As CartaConcessao
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)
        ''' <summary>
        ''' Calcula o tempo de contribui��o do segurado at� a DER do benef�cio. Considerandos apenas os v�nculos n�o concomitantes
        ''' </summary>
        ''' <param name="DER"></param>
        ''' <param name="Autor"></param>
        Friend Sub CalculaTempoContribuicao(DER As Date, Autor As Pessoa)
            Dim TotalDeDias As Integer
            For Each vinc In Autor.Vinculos

                If vinc.Fim <> "" OrElse vinc.UltimaRemuneracao <> "" Then 'Se tem data final ou �ltima remunera��o
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.UltimaRemuneracao)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If
                    If DataInicial > DER Then Continue For
                    If DataFinal > DER Then DataFinal = DER

                    Dim InicialAbsorvido As Boolean = False
                    Dim FinalAbsorvido As Boolean = False

                    For Each vinc2 In Autor.Vinculos 'An�lise se o v�nculo � concomitante com outros, para fins de contagem do tempo de servi�o
                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se n�o � o mesmo v�nculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.UltimaRemuneracao)
                                DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                            Else
                                DataFinal2 = CDate(vinc2.Fim)
                            End If
                            If DataInicial >= DataInicial2 AndAlso DataInicial < DataFinal2 Then
                                InicialAbsorvido = True
                                If DataFinal <= DataFinal2 Then
                                    DataInicial = DataFinal
                                Else
                                    DataInicial = DataFinal2
                                End If
                            End If
                            If DataFinal <= DataFinal2 AndAlso DataFinal > DataInicial2 Then
                                FinalAbsorvido = True
                                If DataInicial >= DataInicial2 Then
                                    DataFinal = DataInicial
                                Else
                                    DataFinal = DataInicial2
                                End If
                            End If
                        End If
                    Next

                    Dim Dias As Integer
                    If InicialAbsorvido And FinalAbsorvido Then
                        Dias = 0
                    Else
                        Dias = Dias360(DataInicial, DataFinal, True) + 1
                    End If
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

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim b As Beneficio = DirectCast(obj, Beneficio)
            If (b.NB = Me.NB) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
    Public Class Vinculo
        Public Property Sequencial As Integer
        Public Property NIT As String
        Public Property CNPJ As String
        Public Property Origem As String
        Public Property Inicio As String
        Public Property Fim As String
        Public Property Filiacao As String
        Public Property Ocupacao As String
        Public Property UltimaRemuneracao As String
        Public Property Anos As Integer
        Public Property Meses As Integer
        Public Property Dias As Integer
        Public Property DiasTotais As Integer
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remuneracoes As New List(Of Remuneracao)
        Public Property Recolhimentos As New List(Of Recolhimento)
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim v As Vinculo = DirectCast(obj, Vinculo)
            If (v.Sequencial = Me.Sequencial) AndAlso (v.NIT = Me.NIT) AndAlso (v.CNPJ = Me.CNPJ) AndAlso (v.Inicio = Me.Inicio) AndAlso (v.Fim = Me.Fim) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
    Public Class Remuneracao
        Public Property Competencia As String
        Public Property Remuneracao As Double
        Public Property Indicadores As New List(Of Indicador)

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim r As Remuneracao = DirectCast(obj, Remuneracao)
            If (r.Competencia = Me.Competencia) AndAlso (r.Remuneracao = Me.Remuneracao) Then
                Return True
            Else
                Return False
            End If
        End Function

    End Class
    Public Class Recolhimento
        Public Property Competencia As String
        Public Property DataPagamento As String
        Public Property Contribuicao As Double
        Public Property SalarioContribuicao As Double
        Public Property Indicadores As New List(Of Indicador)
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim r As Recolhimento = DirectCast(obj, Recolhimento)
            If (r.Competencia = Me.Competencia) AndAlso (r.Contribuicao = Me.Contribuicao) AndAlso (r.DataPagamento = Me.DataPagamento) AndAlso (r.SalarioContribuicao = Me.SalarioContribuicao) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
    Public Class Indicador
        Public Property Indicador As String
        Public Property Descricao As String

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim i As Indicador = DirectCast(obj, Indicador)
            If (i.Indicador = Me.Indicador) AndAlso i.Descricao = Me.Descricao Then
                Return True
            Else
                Return False
            End If
        End Function

    End Class
    Public Class CartaConcessao
        Public Property Despacho As String
        Public Property Resumo As String
        Public Property Salarios As New List(Of SalarioConcessao)

    End Class
    Public Class SalarioConcessao
        Public Property Sequencial As Integer
        Public Property Competencia As String
        Public Property Salario As Double
        Public Property Indice As Double
        Public Property Corrigido As Double
        Public Property Observacao As String
    End Class
    Public Class HISCRE
        Public Property Competencia As String
        Public Property Inicial As String
        Public Property Final As String
        Public Property Bruto As Double
        Public Property Liquido As Double
        Public Property MR As Double
        Public Property Descontos As Double
        Public Property Meio As String
        Public Property Status As String
        Public Property Previsao As String
        Public Property Pagamento As String
        Public Property Invalidado As Boolean
        Public Property Isento As Boolean
        Public Property Creditos As New List(Of Credito)
    End Class
    Public Class Credito
        Public Property Rubrica As Integer
        Public Property Descricao As String
        Public Property Valor As Double
    End Class
    Public Class Laudo
        Public Property Beneficio As String
        Public Property NB As String
        Public Property Requerimento As String
        Public Property Ocupacao As String
        Public Property DataExame As String
        Public Property DER As String
        Public Property DIB As String
        Public Property DID As String
        Public Property DII As String
        Public Property DCB As String
        Public Property CID As String
        Public Property Historico As String
        Public Property ExameFisico As String
        Public Property Consideracoes As String
        Public Property Resultado As String
        Public Property EncaminhaReabilitacao As Boolean
        Public Property AcidenteTrabalho As Boolean
        Public Property AuxilioAcidente As String
        Public Property IsencaoCarencia As Boolean
        Public Property SugestaoLI As Boolean
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
        NaoIdentificada = 0
        FichaSintetica = 1
        RelacaoProcessos = 2
        Requerimentos = 3
        RelacoesPrevidenciarias = 4
        Remuneracoes = 5
        CartaConcessao = 6
        HISCRE = 7
        Laudos = 8
    End Enum
    Public Enum TipoDossie
        Invalido = 0
        Previdenciario = 1
        Medico = 2
    End Enum
End Namespace