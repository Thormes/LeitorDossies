Imports HtmlAgilityPack

Namespace Minerador

    Public Class DossiePrevidenciario
        Public Property Sucesso As Boolean
        Public Property Mensagem As String
        Public Property TipoDeDossie As TipoDossie
        Public Property Autor As Pessoa
        ''' <summary>
        ''' Cria o dossiê, necessário informar no mínimo o conteúdo html do dossiê (código-fonte da página)
        ''' </summary>
        ''' <param name="html"></param>
        Public Sub New(html As String)
            Sucesso = False
            TipoDeDossie = 0
            Autor = New Pessoa
            LerDossie(html)
        End Sub
        ''' <summary>
        ''' Sobrecarga pra realizar a leitura do dossiê já informando uma leitura anterior, para acréscimo e atualização de informações
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
        ''' Realiza a leitura do html para identificar se é dossiê previdenciário ou médico e realiza o preenchimento dos dados do dossiê com os dados do segurado
        ''' Se fornecido uma pessoa, realizar adição dos dados, se não cria um novo dossiê. Ideal realizar primeiro leitura do dossiê previdenciário e posteriormente dossiê médico, para complementar os dados do benefício com os laudos encontrados
        ''' </summary>
        ''' <param name="html"></param>
        ''' <param name="Pessoa"></param>
        Friend Sub LerDossie(html As String, Optional Pessoa As Pessoa = Nothing)
            If Pessoa Is Nothing Then Pessoa = New Pessoa
            html = EliminarQuebras(html)
            html = NormalizarEspacos(html)

            If html.Contains("<title>SAPIENS - Dossiê Previdênciário</title>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Previdenciário
                Mensagem = "Dossiê Previdenciário"
            ElseIf html.Contains("<b>DOSSIÊ MÉDICO</b>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Médico
                Mensagem = "Dossiê Médico"
            Else
                Sucesso = False
                TipoDeDossie = TipoDossie.Inválido
                Mensagem = "Dossiê não reconhecido"
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
                    Mensagem &= Environment.NewLine & "Erro ao realizar a leitura do Dossiê: " & ex.StackTrace
                End Try
            End If

        End Sub
        ''' <summary>
        ''' Calcula o tempo de contribuição do segurado, utilizando como parâmetro os vínculos empregatícios detectados nos dossiês
        ''' A contagem leva em consideração ano de 360 dias e mês de 30 dias. Período concomitante é computado apenas uma única vez
        ''' </summary>
        Friend Sub CalculaTempoContribuicao()
            Dim TotalDeDias As Integer
            For Each vinc In Autor.Vínculos
                If vinc.Fim <> "" OrElse vinc.ÚltimaRemuneração <> "" Then 'Se tem data final ou última remuneração
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.ÚltimaRemuneração)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If

                    For Each vinc2 In Autor.Vínculos 'Análise se o vínculo é concomitante com outros, para fins de contagem do tempo de serviço
                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se não é o mesmo vínculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.ÚltimaRemuneração)
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
            Autor.TempoContribuição = String.Format("{0} Anos, {1} Meses e {2} Dias", AnosTotais, MesesTotais, DiasTotaisRestantes)

        End Sub
        ''' <summary>
        ''' Para facilitação do acesso aos dados, escolhe o último benefício indeferido, o último benefício cessado e a lista de benefícios ativos do segurado
        ''' </summary>
        Friend Sub ProcessaBeneficios()
            Dim benefCessado As New Benefício
            Dim benefIndeferido As New Benefício
            Dim benefAtivo As New List(Of Benefício)
            For Each benef In Autor.Benefícios
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
            Autor.ÚltimoBenefícioCessado = benefCessado
            Autor.ÚltimoBenefícioIndeferido = benefIndeferido
            Autor.BenefíciosAtivos = benefAtivo
        End Sub
    End Class
    Public Class OutroProcesso
        Public Property Processo As String
        Public Property Assunto As String
        Public Property Interessados As String
        Public Property OrgãoJulgador As String
        Public Property Ajuizamento As String
        Public Property DataAbertura As String

    End Class
    Public Class Pessoa
        Public Property Nome As String
        Public Property Nascimento As String
        Public Property NIT As New List(Of String)
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filiação As String
        Public Property EndereçoPrincipal As String
        Public Property EndereçoSecundário As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Benefícios As New List(Of Benefício)
        Public Property Vínculos As New List(Of Vínculo)
        Public Property ÚltimoBenefícioIndeferido As Benefício
        Public Property ÚltimoBenefícioCessado As Benefício
        Public Property BenefíciosAtivos As List(Of Benefício)
        Public Property TempoContribuição As String
        Public Property TotalDias As Integer

    End Class
    Public Class Benefício
        Public Property Sequencial As Integer
        Public Property NB As String
        Public Property Espécie As String
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
        Public Property DataÓbito As String
        Public Property IRT As Double
        Public Property Indice1298 As Double
        Public Property Indice0104 As Double
        Public Property Filiação As String
        Public Property RamoAtividade As String
        Public Property NaturezaOcupação As String
        Public Property TipoConcessão As String
        Public Property Tratamento As Integer
        Public Property DRD As String
        Public Property APSConcessora As String
        Public Property APSMantenedora As String
        Public Property APSRequerimento As String
        Public Property ÓbitoInstituidor As String
        Public Property APR As Double
        Public Property TempoAteDER As String
        Public Property CartaConcessão As CartaConcessão
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)
        ''' <summary>
        ''' Calcula o tempo de contribuição do segurado até a DER do benefício. Considerandos apenas os vínculos não concomitantes
        ''' </summary>
        ''' <param name="DER"></param>
        ''' <param name="Autor"></param>
        Friend Sub CalculaTempoContribuicao(DER As Date, Autor As Pessoa)
            Dim TotalDeDias As Integer
            For Each vinc In Autor.Vínculos

                If vinc.Fim <> "" OrElse vinc.ÚltimaRemuneração <> "" Then 'Se tem data final ou última remuneração
                    Dim DataInicial As Date = CDate(vinc.Inicio)
                    Dim DataFinal As Date
                    If vinc.Fim = "" Then
                        DataFinal = CDate(vinc.ÚltimaRemuneração)
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = CDate(vinc.Fim)
                    End If
                    If DataInicial > DER Then Continue For
                    If DataFinal > DER Then DataFinal = DER

                    For Each vinc2 In Autor.Vínculos 'Análise se o vínculo é concomitante com outros, para fins de contagem do tempo de serviço
                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se não é o mesmo vínculo
                            Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                            Dim DataFinal2 As Date
                            If vinc2.Fim = "" Then
                                DataFinal2 = CDate(vinc2.ÚltimaRemuneração)
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
    Public Class Vínculo
        Public Property Sequencial As Integer
        Public Property NIT As String
        Public Property CNPJ As String
        Public Property Origem As String
        Public Property Inicio As String
        Public Property Fim As String
        Public Property Filiação As String
        Public Property Ocupação As String
        Public Property ÚltimaRemuneração As String
        Public Property Anos As Integer
        Public Property Meses As Integer
        Public Property Dias As Integer
        Public Property DiasTotais As Integer
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remunerações As New List(Of Remuneração)
        Public Property Recolhimentos As New List(Of Recolhimento)

    End Class
    Public Class Remuneração
        Public Property Competência As String
        Public Property Remuneração As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class Recolhimento
        Public Property Competência As String
        Public Property DataPagamento As String
        Public Property Contribuição As Double
        Public Property SalárioContribuição As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class Indicador
        Public Property Indicador As String
        Public Property Descrição As String
    End Class
    Public Class CartaConcessão
        Public Property Despacho As String
        Public Property Resumo As String
        Public Property Salários As New List(Of SalárioConcessão)

    End Class
    Public Class SalárioConcessão
        Public Property Sequencial As Integer
        Public Property Competência As String
        Public Property Salário As Double
        Public Property Índice As Double
        Public Property Corrigido As Double
        Public Property Observação As String
    End Class
    Public Class HISCRE
        Public Property Competência As String
        Public Property Inicial As String
        Public Property Final As String
        Public Property Bruto As Double
        Public Property Líquido As Double
        Public Property MR As Double
        Public Property Descontos As Double
        Public Property Meio As String
        Public Property Status As String
        Public Property Previsão As String
        Public Property Pagamento As String
        Public Property Invalidado As Boolean
        Public Property Isento As Boolean
        Public Property Créditos As New List(Of Crédito)
    End Class
    Public Class Crédito
        Public Property Rubrica As Integer
        Public Property Descrição As String
        Public Property Valor As Double
    End Class
    Public Class Laudo
        Public Property Benefício As String
        Public Property NB As String
        Public Property Requerimento As String
        Public Property Ocupação As String
        Public Property DataExame As String
        Public Property DER As String
        Public Property DIB As String
        Public Property DID As String
        Public Property DII As String
        Public Property DCB As String
        Public Property CID As String
        Public Property Histórico As String
        Public Property ExameFísico As String
        Public Property Considerações As String
        Public Property Resultado As String
        Public Property EncaminhaReabilitação As Boolean
        Public Property AcidenteTrabalho As Boolean
        Public Property AuxilioAcidente As String
        Public Property IsençãoCarencia As Boolean
        Public Property SugestãoLI As Boolean
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
        NãoIdentificada = 0
        FichaSintética = 1
        RelaçãoProcessos = 2
        Requerimentos = 3
        RelaçõesPrevidenciárias = 4
        Remunerações = 5
        CartaConcessão = 6
        HISCRE = 7
        Laudos = 8
    End Enum
    Public Enum TipoDossie
        Inválido = 0
        Previdenciário = 1
        Médico = 2
    End Enum
End Namespace