Imports HtmlAgilityPack
Imports System.Text.RegularExpressions
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

            If html.Contains("<title>SAPIENS - Dossi� Previd�nci�rio</title>") Or html.Contains("<title>SAPIENS - Extrato dos Dossi�s de Defesa e M�dico</title>") Then
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
                    CalculaTempoContribuicao(Pessoa)
                    ProcessaBeneficios()
                Catch ex As Exception
                    Sucesso = False
                    Mensagem &= Environment.NewLine & "Erro ao realizar a leitura do Dossi�: " & ex.StackTrace
                End Try
            End If

        End Sub
        ''' <summary>
        ''' Para facilita��o do acesso aos dados, escolhe o �ltimo benef�cio indeferido, o �ltimo benef�cio cessado e a lista de benef�cios ativos do segurado
        ''' </summary>
        Friend Sub ProcessaBeneficios()
            Dim benefCessado As Beneficio
            Dim benefIndeferido As Beneficio
            Dim benefMensalidade As Beneficio
            Dim benefRequerido As Beneficio
            Dim benefAtivo As New List(Of Beneficio)
            'Autor.Beneficios.Sort(Function(x, y) CDate(x.DER).CompareTo(CDate(y.DER)))
            For Each benef In Autor.Beneficios
                'If benef.Laudos.Count > 0 Then
                '    benef.Laudos.Sort(Function(x, y) CDate(x.DataExame).CompareTo(CDate(y.DataExame)))
                'End If
                calcQualidadeSegurado(benef, Autor)
                If benefRequerido IsNot Nothing Then
                    If benef.DER IsNot Nothing Then
                        If benefRequerido.DER Is Nothing Then
                            benefRequerido = benef
                        Else
                            If benefRequerido.DER <= benef.DER Then benefRequerido = benef
                        End If
                    End If
                Else
                    benefRequerido = benef
                End If
                If benef.DER IsNot Nothing AndAlso Autor.Nascimento IsNot Nothing Then
                    benef.IdadeNaDER = CalculaIdade(Autor.Nascimento, benef.DER)
                End If


                If benef.Status IsNot Nothing Then
                    If benef.Status = "INDEFERIDO" And benef.DER IsNot Nothing Then
                        If benefIndeferido Is Nothing Then
                            benefIndeferido = benef
                        Else
                            If benefIndeferido.DER <= benef.DER Then benefIndeferido = benef
                        End If
                    ElseIf benef.Status = "CESSADO" And benef.DCB IsNot Nothing Then
                        If benefCessado Is Nothing Then
                            benefCessado = benef
                        Else
                            If benefCessado.DCB <= benef.DCB Then benefCessado = benef
                        End If
                    ElseIf benef.Status = "ATIVO" Then
                        If benef.DCB IsNot Nothing Then
                            If benef.DCB <= Today Then
                                benefCessado = benef
                            Else
                                benefAtivo.Add(benef)
                            End If
                        Else
                            benefAtivo.Add(benef)
                        End If

                    ElseIf benef.Status.Contains("RECEBENDO MENSALIDADE") Then
                        If benefMensalidade Is Nothing And benef.DCB IsNot Nothing Then
                            benefMensalidade = benef
                        Else
                            If benefMensalidade.DCB <= benef.DCB Then benefMensalidade = benef
                        End If
                    End If
                    If benef.DER IsNot Nothing Then CalculaTempoContribuicao(Autor, benef)
                End If
            Next
            If benefCessado IsNot Nothing Then Autor.UltimoBeneficioCessado = benefCessado
            If benefIndeferido IsNot Nothing Then Autor.UltimoBeneficioIndeferido = benefIndeferido
            Autor.BeneficiosAtivos = benefAtivo
            If benefMensalidade IsNot Nothing Then Autor.BeneficioEmMensalidadeRecuperacao = benefMensalidade
            If benefRequerido IsNot Nothing Then Autor.UltimoBeneficioRequerido = benefRequerido
        End Sub
    End Class
    Public Class OutroProcesso
        Public Property Processo As String
        Public Property Assunto As String
        Public Property Interessados As String
        Public Property OrgaoJulgador As String
        Public Property Ajuizamento As String
        Public Property DataAbertura As Nullable(Of Date)
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim p As OutroProcesso = DirectCast(obj, OutroProcesso)
            If (p.Processo = Me.Processo) Then
                Return True
            Else
                Return False
            End If

        End Function
    End Class
    Public Class Pessoa
        Public Property Nome As String
        Public Property Nascimento As Nullable(Of Date)
        Public Property NIT As New List(Of String)
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filiacao As String
        Public Property Sexo As String
        Public Property EnderecoPrincipal As String
        Public Property EnderecoSecundario As String
        Public Property EnderecoReceita As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Beneficios As New List(Of Beneficio)
        Public Property Vinculos As New List(Of Vinculo)
        Public Property UltimoBeneficioRequerido As Beneficio
        Public Property UltimoBeneficioIndeferido As Beneficio
        Public Property UltimoBeneficioCessado As Beneficio
        Public Property BeneficioEmMensalidadeRecuperacao As Beneficio
        Public Property BeneficiosAtivos As List(Of Beneficio)
        Public Property TempoContribuicao As String
        Public Property TotalDias As Integer
        Public Property QualidadeSegurado As New List(Of QualidadeSegurado)
    End Class
    Public Class Beneficio
        Public Property Sequencial As Integer
        Public Property NB As String
        Public Property Especie As String
        Public Property DER As Nullable(Of Date)
        Public Property DIB As Nullable(Of Date)
        Public Property DDB As Nullable(Of Date)
        Public Property DCB As Nullable(Of Date)
        Public Property DIP As Nullable(Of Date)
        Public Property Status As String
        Public Property Motivo As String
        Public Property RMI As Double
        Public Property SB As Double
        Public Property Coeficiente As Double
        Public Property RMA As Double
        Public Property DAT As Nullable(Of Date)
        Public Property DataNBAnterior As Nullable(Of Date)
        Public Property DataObito As Nullable(Of Date)
        Public Property IRT As Double
        Public Property Indice1298 As Double
        Public Property Indice0104 As Double
        Public Property Filiacao As String
        Public Property RamoAtividade As String
        Public Property NaturezaOcupacao As String
        Public Property TipoConcessao As String
        Public Property Tratamento As Integer
        Public Property DRD As Nullable(Of Date)
        Public Property APSConcessora As String
        Public Property APSMantenedora As String
        Public Property APSRequerimento As String
        Public Property ObitoInstituidor As Nullable(Of Date)
        Public Property APR As Double
        Public Property TempoAteDER As String
        Public Property IdadeNaDER As String
        Public Property CartaConcessao As CartaConcessao
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)
        Public Property DespachoDecisorio As String
    End Class
    Public Class Vinculo
        Public Property Sequencial As Integer
        Public Property NIT As String
        Public Property CNPJ As String
        Public Property Origem As String
        Public Property Inicio As Nullable(Of Date)
        Public Property Fim As Nullable(Of Date)
        Public Property Concomitancia As String
        Public Property Filiacao As String
        Public Property Ocupacao As String
        Public Property UltimaRemuneracao As Nullable(Of Date)
        Public Property Anos As Integer
        Public Property Meses As Integer
        Public Property Dias As Integer
        Public Property DiasTotais As Integer
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remuneracoes As New List(Of Remuneracao)
        Public Property Recolhimentos As New List(Of Recolhimento)
    End Class
    Public Class Remuneracao
        Public Property Competencia As Nullable(Of Date)
        Public Property Remuneracao As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class Recolhimento
        Public Property Competencia As Nullable(Of Date)
        Public Property DataPagamento As Nullable(Of Date)
        Public Property Contribuicao As Double
        Public Property SalarioContribuicao As Double
        Public Property Indicadores As New List(Of Indicador)
    End Class
    Public Class QualidadeSegurado
        Public Property Inicio As Nullable(Of Date)
        Public Property Fim As Nullable(Of Date)
        Public Property tipodeQualidade As tipoQualidade
        Public Property Extensao As Integer


        Public Enum tipoQualidade As Integer
            Beneficio = 1
            Atividade = 2
            Segregacao = 3
            Reclusao = 4
            Licenciamento = 5
            Facultativo = 6
        End Enum
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
        Public Property Competencia As Nullable(Of Date)
        Public Property Salario As Double
        Public Property Indice As Double
        Public Property Corrigido As Double
        Public Property Observacao As String
    End Class
    Public Class HISCRE
        Public Property Competencia As Nullable(Of Date)
        Public Property Inicial As Nullable(Of Date)
        Public Property Final As Nullable(Of Date)
        Public Property Bruto As Double
        Public Property Liquido As Double
        Public Property MR As Double
        Public Property Descontos As Double
        Public Property Meio As String
        Public Property Status As String
        Public Property Previsao As Nullable(Of Date)
        Public Property Pagamento As Nullable(Of Date)
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
        Public Property DataExame As Nullable(Of Date)
        Public Property DER As Nullable(Of Date)
        Public Property DIB As Nullable(Of Date)
        Public Property DID As Nullable(Of Date)
        Public Property DII As Nullable(Of Date)
        Public Property DCB As Nullable(Of Date)
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