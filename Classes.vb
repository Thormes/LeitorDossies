Imports HtmlAgilityPack
Namespace Minerador
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
    Public Class DossiePrevidenciario
        Public Property Sucesso As Boolean
        Public Property Mensagem As String
        Public Property TipoDeDossie As TipoDossie
        Public Property Autor As Pessoa
        Public Sub New()
            Sucesso = False
            TipoDeDossie = 0
            Autor = New Pessoa
        End Sub
        Public Sub New(Pessoa As Pessoa)
            Sucesso = False
            TipoDeDossie = 0
            Autor = Pessoa
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
        Public Property NIT As String
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filiação As String
        Public Property EndereçoPrincipal As String
        Public Property EndereçoSecundário As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Benefícios As New List(Of Benefício)
        Public Property Vínculos As New List(Of Vínculo)

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
        Public Property CartaConcessão As CartaConcessão
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)

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
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remunerações As New List(Of Remuneração)

    End Class
    Public Class Remuneração
        Public Property Competência As String
        Public Property Remuneração As Double
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
End Namespace