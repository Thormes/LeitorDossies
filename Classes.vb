Imports HtmlAgilityPack
Namespace Minerador
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
        Public Property Org�oJulgador As String
        Public Property Ajuizamento As String
        Public Property DataAbertura As String

    End Class
    Public Class Pessoa
        Public Property Nome As String
        Public Property Nascimento As String
        Public Property NIT As String
        Public Property CPF As String
        Public Property EstadoCivil As String
        Public Property Filia��o As String
        Public Property Endere�oPrincipal As String
        Public Property Endere�oSecund�rio As String
        Public Property OutrosProcessos As New List(Of OutroProcesso)
        Public Property Benef�cios As New List(Of Benef�cio)
        Public Property V�nculos As New List(Of V�nculo)

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
        Public Property CartaConcess�o As CartaConcess�o
        Public Property Laudos As New List(Of Laudo)
        Public Property HISCRE As New List(Of HISCRE)

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
        Public Property Indicadores As New List(Of Indicador)
        Public Property Remunera��es As New List(Of Remunera��o)

    End Class
    Public Class Remunera��o
        Public Property Compet�ncia As String
        Public Property Remunera��o As Double
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
End Namespace