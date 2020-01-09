Imports System.Text
Imports System.Text.RegularExpressions
Module StrTools
    Function GetStringInBetween(ByVal strBegin As String, ByVal strEnd As String, ByVal strSource As String, ByVal includeBegin As Boolean, ByVal includeEnd As Boolean) As String
        Dim result As String = ""
        Dim iBegin As Integer = strSource.IndexOf(strBegin)
        Dim iEnd As Integer = strSource.IndexOf(strEnd, iBegin + strBegin.Length)
        If iBegin = -1 OrElse iEnd = -1 Then Return result
        If iBegin + strBegin.Length >= iEnd Then Return result
        If Not includeBegin Then iBegin += strBegin.Length
        strSource = strSource.Substring(iBegin)
        iEnd = strSource.IndexOf(strEnd)
        If includeEnd Then iEnd += strEnd.Length
        result = strSource.Substring(0, iEnd)
        Return result
    End Function

    Function NormalizarEspacos(ByVal t As String) As String
        While t.Contains("  ")
            t = t.Replace("  ", " ")
        End While

        Return t
    End Function

    Friend Function EliminarQuebrasEspacos(ByVal textoTabela As String) As String
        textoTabela = textoTabela.Replace(" ", "")
        textoTabela = EliminarQuebras(textoTabela)
        Return textoTabela
    End Function

    Friend Function EliminarQuebras(ByVal texto As String) As String
        texto = texto.Replace(System.Environment.NewLine, "")
        texto = texto.Replace(vbLf, "")
        Return texto
    End Function

    Friend Function SomenteAlphaNumerico(ByVal texto As String, ByVal Optional excecao As String = "") As String
        Dim rgx As Regex = New Regex("[^a-zA-Z0-9 -" & excecao & "]")
        texto = rgx.Replace(texto, "")
        Return texto
    End Function
    Friend Function SomenteNumerico(ByVal texto As String) As String
        Dim rgx As Regex = New Regex("[^0-9]")
        texto = rgx.Replace(texto, "")
        Return texto
    End Function

    Friend Function RetirarAcentuacao(ByVal texto As String) As String
        Dim toReplace As Char() = "àèìòùÀÈÌÒÙ äëïöüÄËÏÖÜ âêîôûÂÊÎÔÛ áéíóúÁÉÍÓÚðÐýÝ ãñõÃÑÕšŠžŽçÇåÅøØ".ToCharArray()
        Dim replaceChars As Char() = "aeiouAEIOU aeiouAEIOU aeiouAEIOU aeiouAEIOUdDyY anoANOsSzZcCaAoO".ToCharArray()

        For index As Integer = 0 To toReplace.GetUpperBound(0)
            texto = texto.Replace(toReplace(index), replaceChars(index))
        Next

        Return texto
    End Function

    Friend Function SeparadoresPorUnderline(ByVal texto As String) As String
        Dim seps As String() = {"(", "/", "|", "\", ".", "{", "-", "["}

        For Each s As String In seps
            texto = texto.Replace(s, "_")
        Next

        Return texto
    End Function

    Friend Function TratarNomeVariavel(ByVal texto As String) As String
        texto = EliminarQuebrasEspacos(texto)
        texto = SeparadoresPorUnderline(texto)
        texto = RetirarAcentuacao(texto)
        texto = SomenteAlphaNumerico(texto, "_")
        Return texto
    End Function
    Friend Function parseBool(ByVal texto As String) As Boolean
        If texto = "SIM" Then Return True
        If texto = "NÃO" Then Return False
        Return False
    End Function
End Module

