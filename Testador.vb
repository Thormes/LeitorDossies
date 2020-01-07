Imports System.Xml
Imports System.Xml.Linq
Imports HtmlAgilityPack
Imports LeitorDossies.Minerador
Imports Newtonsoft.Json
Imports Newtonsoft
Imports Newtonsoft.Json.Linq

Public Class Testador
    Public Autor As New Pessoa
    Private Sub LerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LerToolStripMenuItem.Click
        Dim textoEntrada As String = txtEntrada.Text
        textoEntrada = EliminarQuebras(textoEntrada)
        textoEntrada = NormalizarEspacos(textoEntrada)
        Dim novoDossie As New DossiePrevidenciario
        If textoEntrada.Contains("<title>SAPIENS - Dossiê Previdênciário</title>") Then
            novoDossie.Sucesso = True
            novoDossie.TipoDeDossie = TipoDossie.Previdenciário
        End If
        If novoDossie.Sucesso = True Then

            Dim textoHTML As New HtmlDocument()
            textoHTML.LoadHtml(textoEntrada)
            Dim tabelas As HtmlNodeCollection = textoHTML.DocumentNode.SelectNodes("//table")
            Dim quebralinha As String = Environment.NewLine & "-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & Environment.NewLine
            Dim listaTabelas As New List(Of Integer)
            For Each tabela In tabelas

                Dim teste As ParseTabelaResult
                teste = parseTabelas.IdentificarTabela(tabela)


                If teste.IdTabela > 0 And listaTabelas.Contains(teste.IdTabela) = False Then
                    ParseTabela(teste.IdTabela, teste.Elemento, novoDossie.Autor)
                    listaTabelas.Add(teste.IdTabela)
                End If
            Next
            Dim json = JsonConvert.SerializeObject(novoDossie.Autor, Newtonsoft.Json.Formatting.Indented)
            Autor = novoDossie.Autor
            txtSaída.Text = json
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "/" & novoDossie.Autor.Nome & ".json", json)

        Else
            MsgBox("Não é Dossiê!")
        End If
    End Sub

    Private Sub LimparToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LimparToolStripMenuItem.Click
        txtEntrada.Text = ""
        txtSaída.Text = ""
    End Sub

    Private Sub DossiêToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DossiêToolStripMenuItem.Click
        LeituraParcial(TipoTabela.FichaSintética)
    End Sub
    Private Sub LeituraParcial(tipoTabela As TipoTabela)
        Dim textoEntrada As String = txtEntrada.Text
        textoEntrada = EliminarQuebras(textoEntrada)
        textoEntrada = NormalizarEspacos(textoEntrada)
        Dim novoDossie As New Minerador.DossiePrevidenciario
        If textoEntrada.Contains("<title>SAPIENS - Dossiê Previdênciário</title>") Then
            novoDossie.Sucesso = True
            novoDossie.TipoDeDossie = TipoDossie.Previdenciário
        End If
        If novoDossie.Sucesso = True Then

            Dim textoHTML As New HtmlDocument()
            textoHTML.LoadHtml(textoEntrada)
            Dim tabelas As HtmlNodeCollection = textoHTML.DocumentNode.SelectNodes("//table")
            Dim quebralinha As String = Environment.NewLine & "-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & Environment.NewLine
            Dim listaTabelas As New List(Of Integer)
            For Each tabela In tabelas

                Dim teste As ParseTabelaResult
                teste = parseTabelas.IdentificarTabela(tabela)
                If teste.IdTabela = tipoTabela And listaTabelas.Contains(teste.IdTabela) = False Then
                    ParseTabela(teste.IdTabela, teste.Elemento, novoDossie.Autor)
                    listaTabelas.Add(teste.IdTabela)
                End If
            Next
            Dim json = JsonConvert.SerializeObject(novoDossie.Autor)

            txtSaída.Text = json

        Else
            MsgBox("Não é Dossiê!")
        End If
    End Sub

    Private Sub BenefíciosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BenefíciosToolStripMenuItem.Click
        Dim textoEntrada As String = txtEntrada.Text
        textoEntrada = EliminarQuebras(textoEntrada)
        textoEntrada = NormalizarEspacos(textoEntrada)
        Dim novoDossie As New Minerador.DossiePrevidenciario
        If textoEntrada.Contains("<title>SAPIENS - Dossiê Previdênciário</title>") Then
            novoDossie.Sucesso = True
            novoDossie.TipoDeDossie = TipoDossie.Previdenciário
        End If
        If novoDossie.Sucesso = True Then

            Dim textoHTML As New HtmlDocument()
            textoHTML.LoadHtml(textoEntrada)
            Dim tabelas As HtmlNodeCollection = textoHTML.DocumentNode.SelectNodes("//table")
            Dim quebralinha As String = Environment.NewLine & "-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & Environment.NewLine
            Dim listaTabelas As New List(Of Integer)
            For Each tabela In tabelas

                Dim teste As ParseTabelaResult
                teste = parseTabelas.IdentificarTabela(tabela)
                If teste.IdTabela > 0 And listaTabelas.Contains(teste.IdTabela) = False Then
                    ParseTabela(teste.IdTabela, teste.Elemento, novoDossie.Autor)
                    listaTabelas.Add(teste.IdTabela)
                End If
            Next
            Dim json = JsonConvert.SerializeObject(novoDossie.Autor.Benefícios)

            txtSaída.Text = json

        Else
            MsgBox("Não é Dossiê!")
        End If
    End Sub

    Private Sub HISCREToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HISCREToolStripMenuItem.Click
        LeituraParcial(TipoTabela.CartaConcessão)
    End Sub

    Private Sub HISCREToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles HISCREToolStripMenuItem1.Click
        LeituraParcial(TipoTabela.HISCRE)
    End Sub

    Private Sub copyClipboard_Click(sender As Object, e As EventArgs) Handles copyClipboard.Click
        Clipboard.SetText(txtSaída.Text)
    End Sub

    Private Sub MédicoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MédicoToolStripMenuItem.Click
        Dim textoEntrada As String = txtEntrada.Text
        textoEntrada = EliminarQuebras(textoEntrada)
        textoEntrada = NormalizarEspacos(textoEntrada)
        Dim novoDossie As New DossiePrevidenciario(Autor)
        If textoEntrada.Contains("<b>DOSSIÊ MÉDICO</b>") Then
            novoDossie.Sucesso = True
            novoDossie.TipoDeDossie = TipoDossie.Médico
        End If
        If novoDossie.Sucesso = True Then
            Dim textoHTML As New HtmlDocument()
            textoHTML.LoadHtml(textoEntrada)
            Dim tabelas As HtmlNodeCollection = textoHTML.DocumentNode.SelectNodes("//table")
            Dim quebralinha As String = Environment.NewLine & "-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & Environment.NewLine
            Dim listaTabelas As New List(Of Integer)
            For Each tabela In tabelas

                Dim teste As ParseTabelaResult
                teste = parseTabelas.IdentificarTabela(tabela)
                If teste.IdTabela = 8 And Not listaTabelas.Contains(teste.IdTabela) Then
                    ParseTabela(teste.IdTabela, teste.Elemento, novoDossie.Autor)
                    listaTabelas.Add(teste.IdTabela)
                End If
            Next
            Dim json = JsonConvert.SerializeObject(novoDossie.Autor, Newtonsoft.Json.Formatting.Indented)

            txtSaída.Text = json

        Else
            MsgBox("Não é Dossiê!")
        End If
    End Sub
End Class
