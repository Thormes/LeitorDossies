﻿Imports LeitorDossies.Minerador
Imports HtmlAgilityPack
Imports System.Text.RegularExpressions
Imports System.Net.WebUtility


Module parseTabelas

    Friend Function IdentificarTabela(tabela As HtmlNode) As ParseTabelaResult
        Dim retorno As New ParseTabelaResult

        Dim linhas As HtmlNodeCollection = tabela.SelectNodes(".//tr")
        Dim primeirocabeçalho As HtmlNode = tabela.SelectSingleNode(".//th[0]")
        Dim segundocabeçalho As HtmlNode = tabela.SelectSingleNode(".//th[1]")
        Dim primeirocampo As HtmlNode = tabela.SelectSingleNode(".//td[0]")
        Dim últimocabeçalho As HtmlNode = tabela.SelectSingleNode(".//th[last()]")
        If últimocabeçalho IsNot Nothing Then
            Select Case últimocabeçalho.InnerText
                Case Is = "NÚMERO ÚNICO (CNJ)"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.FichaSintetica
                    retorno.Elemento = tabela
                Case Is = "DATA ABERTURA"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.RelacaoProcessos
                    retorno.Elemento = tabela
                Case Is = "MOTIVO"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.Requerimentos
                    retorno.Elemento = tabela
                Case Is = "INDICADORES"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.RelacoesPrevidenciarias
                    retorno.Elemento = tabela
                Case Is = "Indicadores (*)", "Motivo", "APS Requerimento"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.Remuneracoes
                    retorno.Elemento = tabela.ParentNode.ParentNode
                Case Is = "Data de Concessão do Benefício"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.CartaConcessao
                    retorno.Elemento = tabela.ParentNode.ParentNode.ParentNode
                Case Is = "Data Início Pagamento (DIP)"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.HISCRE
                    retorno.Elemento = tabela.ParentNode.ParentNode.ParentNode
                Case Is = "DATA DO EXAME"
                    retorno.Sucesso = True
                    retorno.IdTabela = TipoTabela.Laudos
                    retorno.Elemento = tabela.ParentNode.ParentNode.ParentNode
                Case Else
                    retorno.Sucesso = False
                    retorno.IdTabela = TipoTabela.NaoIdentificada
                    retorno.Elemento = Nothing
            End Select
        End If
        Return retorno
    End Function
    Friend Sub ParseTabela(idTabela As Integer, Elemento As HtmlNode, Pessoa As Pessoa)
        Select Case idTabela
            Case Is = TipoTabela.FichaSintetica
                ParseTabelaSintetica(Elemento, Pessoa)
            Case Is = TipoTabela.RelacaoProcessos
                ParseTabelaRelaçãoProcessos(Elemento, Pessoa)
            Case Is = TipoTabela.Requerimentos
                ParseTabelaRequerimentos(Elemento, Pessoa)
            Case Is = TipoTabela.RelacoesPrevidenciarias
                ParseTabelaRelações(Elemento, Pessoa)
            Case Is = TipoTabela.Remuneracoes
                ParseTabelaRemunerações(Elemento, Pessoa)
            Case Is = TipoTabela.CartaConcessao
                ParseTabelaCartaConcessão(Elemento, Pessoa)
            Case Is = TipoTabela.HISCRE
                ParseTabelaHISCRE(Elemento, Pessoa)
            Case Is = TipoTabela.Laudos
                ParseTabelaLaudos(Elemento, Pessoa)
        End Select
    End Sub
    Friend Sub ParseTabelaSintetica(Elemento As HtmlNode, ByRef Pessoa As Pessoa)
        Dim linhas As HtmlNodeCollection = Elemento.SelectNodes(".//tr")
        For Each linha In linhas
            If linha.SelectSingleNode(".//th").InnerText = "NIT" Then
                Dim ValorNIT As String = linha.SelectSingleNode(".//td").InnerText.Trim
                If Not Pessoa.NIT.Contains(ValorNIT) Then
                    Pessoa.NIT.Add(ValorNIT)
                End If
            End If
            If linha.SelectSingleNode(".//th").InnerText = "PARTE AUTORA" Then Pessoa.Nome = linha.SelectSingleNode(".//td").InnerText.Trim
            If linha.SelectSingleNode(".//th").InnerText = "CPF" Then Pessoa.CPF = linha.SelectSingleNode(".//td").InnerText.Trim
            If linha.SelectSingleNode(".//th").InnerText = "DATA DE NASCIMENTO" Then Pessoa.Nascimento = linha.SelectSingleNode(".//td").InnerText.Trim
            If linha.SelectSingleNode(".//th").InnerText = "ESTADO CIVIL" Then Pessoa.EstadoCivil = linha.SelectSingleNode(".//td").InnerText
            If linha.SelectSingleNode(".//th").InnerText = "FILIAÇÃO" Then Pessoa.Filiacao = linha.SelectSingleNode(".//td").InnerText
            If linha.SelectSingleNode(".//th").InnerText = "SEXO" Then Pessoa.Sexo = linha.SelectSingleNode(".//td").InnerText
            If linha.SelectSingleNode(".//th").InnerText = "ENDEREÇO PRINCIPAL" Then Pessoa.EnderecoPrincipal = linha.SelectSingleNode(".//td").InnerText
            If linha.SelectSingleNode(".//th").InnerText = "ENDEREÇO SECUNDÁRIO" Then Pessoa.EnderecoSecundario = linha.SelectSingleNode(".//td").InnerText
        Next
    End Sub
    Friend Sub ParseTabelaRelaçãoProcessos(Elemento As HtmlNode, ByRef Pessoa As Pessoa)
        Dim linhas As HtmlNodeCollection = Elemento.SelectNodes(".//tr")
        For Each linha In linhas
            Dim Processo As New OutroProcesso
            If linha.SelectSingleNode(".//th") Is Nothing AndAlso linha.ParentNode.ParentNode.Name <> "td" And linha.ChildNodes.Count > 3 Then
                Processo.Processo = linha.SelectSingleNode(".//td[1]").InnerText.Trim
                Processo.Assunto = linha.SelectSingleNode(".//td[2]").InnerText.Trim
                Processo.Interessados = String.Join("; ", linha.SelectNodes(".//td[3]/table/tr/td").Select(Function(i) i.InnerText.Trim))
                Processo.OrgaoJulgador = linha.SelectSingleNode(".//td[4]").InnerText.Trim
                Processo.Ajuizamento = linha.SelectSingleNode(".//td[5]").InnerText.Trim
                Processo.DataAbertura = linha.SelectSingleNode(".//td[6]").InnerText.Trim
                If Not Pessoa.OutrosProcessos.Contains(Processo) Then Pessoa.OutrosProcessos.Add(Processo)
            End If
        Next
    End Sub
    Friend Sub ParseTabelaRequerimentos(Elemento As HtmlNode, ByRef Pessoa As Pessoa)
        Dim linhas As HtmlNodeCollection = Elemento.SelectNodes(".//tr")
        For Each linha In linhas
            Dim benefício As New Beneficio
            Dim benefícionovo As Boolean = True
            If linha.SelectSingleNode(".//th") Is Nothing Then
                If Not linha.InnerText.Trim.Contains("Não foram encontrados requerimentos em nome do autor") Then
                    benefício.NB = linha.SelectSingleNode(".//td[1]").InnerText.Trim
                    benefício.Especie = linha.SelectSingleNode(".//td[2]").InnerText.Trim
                    benefício.DER = pegaData(linha.SelectSingleNode(".//td[3]").InnerText.Trim)
                    benefício.DIB = pegaData(linha.SelectSingleNode(".//td[4]").InnerText.Trim)
                    benefício.DCB = pegaData(linha.SelectSingleNode(".//td[5]").InnerText.Trim)
                    benefício.Status = linha.SelectSingleNode(".//td[6]").InnerText.Trim
                    benefício.Motivo = HtmlDecode(linha.SelectSingleNode(".//td[7]").InnerText.Trim)
                    For Each benef In Pessoa.Beneficios
                        If benef.NB = benefício.NB Then
                            benefícionovo = False
                        End If
                    Next
                    If benefícionovo Then Pessoa.Beneficios.Add(benefício)
                End If
            End If
        Next
    End Sub
    Friend Sub ParseTabelaRelações(Elemento As HtmlNode, ByRef Pessoa As Pessoa)
        Dim linhas As HtmlNodeCollection = Elemento.SelectNodes(".//tr") 'linhas onde estão os vínculos/benefícios
        Dim tabelaindicadores As HtmlNode = Elemento.OwnerDocument.DocumentNode.SelectSingleNode("//*[contains(text(),'LEGENDA DE INDICADORES')]") 'tabela de indicadores ao final da lista de vínculos
        Dim listaindicadores As New List(Of Indicador)
        If tabelaindicadores IsNot Nothing Then
            Dim b As HtmlNode = tabelaindicadores.ParentNode.ParentNode.ParentNode.SelectSingleNode(".//table")


            Dim indicadores As HtmlNodeCollection = b.SelectNodes(".//tr")
            For Each indicador In indicadores
                If indicador.SelectSingleNode(".//th") Is Nothing Then
                    Dim oindicador As New Indicador With {
                            .Indicador = indicador.SelectSingleNode(".//td[1]").InnerText.Trim,
                            .Descricao = indicador.SelectSingleNode(".//td[2]").InnerText.Trim
                        }
                    listaindicadores.Add(oindicador)
                End If
            Next
        End If
        For Each linha In linhas
            Dim relação As New Vinculo
            Dim novaRelação As Boolean = True
            If linha.SelectSingleNode(".//th") Is Nothing AndAlso linha.SelectSingleNode(".//td[7]").InnerText.Trim <> "Benefício" Then 'Só puxa os vínculos, ignorando os benefícios
                Dim sequencial As String = linha.SelectSingleNode(".//td[1]").InnerText.Trim
                Dim NIT As String = linha.SelectSingleNode(".//td[2]").InnerText.Trim
                For Each vinc In Pessoa.Vinculos
                    If sequencial = vinc.Sequencial And NIT = vinc.NIT Then
                        relação = vinc
                        novaRelação = False
                    End If
                Next

                relação.Sequencial = linha.SelectSingleNode(".//td[1]").InnerText.Trim
                relação.NIT = linha.SelectSingleNode(".//td[2]").InnerText.Trim
                relação.Origem = linha.SelectSingleNode(".//td[4]").InnerText.Trim

                relação.Inicio = pegaData(linha.SelectSingleNode(".//td[5]").InnerText.Trim)
                relação.Fim = pegaData(linha.SelectSingleNode(".//td[6]").InnerText.Trim)
                relação.Filiacao = linha.SelectSingleNode(".//td[7]").InnerText.Trim
                relação.Ocupacao = linha.SelectSingleNode(".//td[8]").InnerText.Trim
                relação.UltimaRemuneracao = pegaData(linha.SelectSingleNode(".//td[9]").InnerText.Trim)
                Dim relindicador = linha.SelectSingleNode(".//td[10]").InnerText
                If relindicador <> "" Then
                    relação.Indicadores = New List(Of Indicador)
                    For Each indicador In listaindicadores
                        Dim indicadoresvinculo = Split(Replace(relindicador.Trim, ",", ""), " ")
                        For i = 0 To indicadoresvinculo.Length - 1
                            If indicador.Indicador = indicadoresvinculo(i) Then
                                If Not relação.Indicadores.Contains(indicador) Then relação.Indicadores.Add(indicador)
                            End If
                        Next
                    Next

                End If
                relação.CNPJ = linha.SelectSingleNode(".//td[3]").InnerText.Trim
                If novaRelação Then Pessoa.Vinculos.Add(relação)
            End If
        Next
    End Sub
    Friend Sub ParseTabelaRemunerações(Elemento As HtmlNode, Pessoa As Pessoa)
        Dim listavinculos As HtmlNodeCollection = Elemento.SelectNodes(".//div[@id='div_competencias']/table/tr")
        Dim tabelaIndicadores As HtmlNode = Elemento.SelectSingleNode(".//preceding::div[1]")
        Dim listaIndicadores As New List(Of Indicador)
        If tabelaIndicadores IsNot Nothing AndAlso tabelaIndicadores.InnerText.Contains("INDICADORES") Then
            Dim linhasIndicadores As HtmlNodeCollection = tabelaIndicadores.SelectNodes(".//table[1]/tr")
            For Each linha In linhasIndicadores
                If linha.SelectSingleNode(".//th") IsNot Nothing Then Continue For
                listaIndicadores.Add(New Indicador With {.Indicador = linha.SelectSingleNode(".//td[1]").InnerText.Trim, .Descricao = linha.SelectSingleNode(".//td[2]").InnerText.Trim})
            Next
        End If

        For Each vinculo In listavinculos
            Dim relaçãonova As Boolean = False, benefícionovo As Boolean = False
            Dim relação As Vinculo
            Dim benefício As Beneficio

            If vinculo.SelectSingleNode(".//th") IsNot Nothing Then Continue For

            Dim Colunas As Integer = vinculo.ChildNodes.Count / 2

            If Colunas < 10 Then 'Não é benefício, é vínculo
                Dim sequencial As Integer = vinculo.SelectSingleNode(".//td[1]").InnerText
                For Each rel In Pessoa.Vinculos
                    If rel.Sequencial = sequencial Then
                        relação = rel
                        Exit For
                    End If
                Next
                If relação Is Nothing Then
                    relação = New Vinculo
                    relaçãonova = True
                End If
                Dim listaremunerações As HtmlNode = vinculo.ParentNode.ParentNode.SelectSingleNode(".//center/b[1]")
                If listaremunerações IsNot Nothing AndAlso (listaremunerações.InnerText = "Lista de Remunerações" Or listaremunerações.InnerText = "Lista de Recolhimentos") Then
                    Dim tabelaremunerações As HtmlNodeCollection
                    If listaremunerações.InnerText = "Lista de Remunerações" Then
                        tabelaremunerações = listaremunerações.ParentNode.SelectNodes(".//table[2]/tr")
                        For Each trremuneração In tabelaremunerações

                            'If relação.Remuneracoes Is Nothing Then relação.Remuneracoes = New List(Of Remuneracao)
                            Dim novaremuneração As Boolean = True
                            Dim remuneração As New Remuneracao
                            Dim competencia As Date = pegaData(trremuneração.SelectSingleNode(".//td[1]").InnerText)
                            Dim remuneracao As String = trremuneração.SelectSingleNode(".//td[3]").InnerText
                            For Each remunera In relação.Remuneracoes
                                If remunera.Competencia = competencia And remuneracao = remunera.Remuneracao Then
                                    remuneração = remunera
                                    novaremuneração = False
                                End If
                            Next
                            With remuneração
                                .Competencia = pegaData(trremuneração.SelectSingleNode(".//td[1]").InnerText)
                                .Remuneracao = trremuneração.SelectSingleNode(".//td[3]").InnerText
                            End With
                            If trremuneração.SelectSingleNode(".//td[4]").InnerText.Trim <> "" Then
                                For Each indicador In listaIndicadores
                                    Dim indicadoresremuneração = Split(Replace(trremuneração.SelectSingleNode(".//td[4]").InnerText.Trim, ",", ""), " ")
                                    For i = 0 To indicadoresremuneração.Length - 1
                                        If indicador.Indicador = indicadoresremuneração(i) Then
                                            If Not remuneração.Indicadores.Contains(indicador) Then remuneração.Indicadores.Add(indicador)
                                        End If
                                    Next
                                Next
                            End If
                            If novaremuneração Then relação.Remuneracoes.Add(remuneração)


                            If trremuneração.SelectSingleNode(".//td[5]") IsNot Nothing Then
                                novaremuneração = True
                                remuneração = New Remuneracao
                                competencia = pegaData(trremuneração.SelectSingleNode(".//td[5]").InnerText)
                                remuneracao = trremuneração.SelectSingleNode(".//td[7]").InnerText
                                For Each remunera In relação.Remuneracoes
                                    If remunera.Competencia = competencia And remuneracao = remunera.Remuneracao Then
                                        remuneração = remunera
                                        novaremuneração = False
                                    End If
                                Next
                                With remuneração
                                    .Competencia = pegaData(trremuneração.SelectSingleNode(".//td[5]").InnerText)
                                    .Remuneracao = trremuneração.SelectSingleNode(".//td[7]").InnerText
                                End With
                                If trremuneração.SelectSingleNode(".//td[8]").InnerText.Trim <> "" Then
                                    For Each indicador In listaIndicadores
                                        Dim indicadoresremuneração = Split(Replace(trremuneração.SelectSingleNode(".//td[8]").InnerText.Trim, ",", ""), " ")
                                        For i = 0 To indicadoresremuneração.Length - 1
                                            If indicador.Indicador = indicadoresremuneração(i) Then
                                                If Not remuneração.Indicadores.Contains(indicador) Then remuneração.Indicadores.Add(indicador)
                                            End If
                                        Next
                                    Next
                                End If

                                If novaremuneração Then relação.Remuneracoes.Add(remuneração)
                            End If

                        Next
                    ElseIf listaremunerações.InnerText = "Lista de Recolhimentos" Then
                        tabelaremunerações = listaremunerações.ParentNode.ParentNode.ParentNode.SelectNodes(".//table[2]/tr")
                        For Each trremuneração In tabelaremunerações
                            'If relação.Recolhimentos Is Nothing Then relação.Recolhimentos = New List(Of Recolhimento)
                            Dim novaremuneração As Boolean = True
                            Dim remuneração As New Recolhimento
                            Dim competencia As Date = pegaData(trremuneração.SelectSingleNode(".//td[1]").InnerText)
                            Dim dataPagamento As Date = pegaData(trremuneração.SelectSingleNode(".//td[2]").InnerText)
                            Dim contribuicao As String = trremuneração.SelectSingleNode(".//td[3]").InnerText
                            For Each remunera In relação.Recolhimentos
                                If remunera.Competencia = competencia And contribuicao = remunera.Contribuicao And dataPagamento = remunera.DataPagamento Then
                                    remuneração = remunera
                                    novaremuneração = False
                                End If
                            Next

                            With remuneração
                                .Competencia = pegaData(trremuneração.SelectSingleNode(".//td[1]").InnerText)
                                .DataPagamento = pegaData(trremuneração.SelectSingleNode(".//td[2]").InnerText)
                                .Contribuicao = trremuneração.SelectSingleNode(".//td[3]").InnerText
                                .SalarioContribuicao = trremuneração.SelectSingleNode(".//td[4]").InnerText
                            End With
                            If trremuneração.SelectSingleNode(".//td[5]").InnerText.Trim <> "" Then
                                For Each indicador In listaIndicadores
                                    Dim indicadoresremuneração = Split(Replace(trremuneração.SelectSingleNode(".//td[5]").InnerText.Trim, ",", ""), " ")
                                    For i = 0 To indicadoresremuneração.Length - 1
                                        If indicador.Indicador = indicadoresremuneração(i) Then
                                            If Not remuneração.Indicadores.Contains(indicador) Then remuneração.Indicadores.Add(indicador)
                                        End If
                                    Next
                                Next
                            End If
                            If novaremuneração Then relação.Recolhimentos.Add(remuneração)
                            If trremuneração.SelectSingleNode(".//td[6]") IsNot Nothing Then
                                novaremuneração = True
                                remuneração = New Recolhimento
                                competencia = pegaData(trremuneração.SelectSingleNode(".//td[6]").InnerText)
                                dataPagamento = pegaData(trremuneração.SelectSingleNode(".//td[7]").InnerText)
                                contribuicao = trremuneração.SelectSingleNode(".//td[8]").InnerText
                                For Each remunera In relação.Recolhimentos
                                    If remunera.Competencia = competencia And contribuicao = remunera.Contribuicao And dataPagamento = remunera.DataPagamento Then
                                        remuneração = remunera
                                        novaremuneração = False
                                    End If
                                Next

                                With remuneração
                                    .Competencia = pegaData(trremuneração.SelectSingleNode(".//td[6]").InnerText)
                                    .DataPagamento = pegaData(trremuneração.SelectSingleNode(".//td[7]").InnerText)
                                    .Contribuicao = trremuneração.SelectSingleNode(".//td[8]").InnerText
                                    .SalarioContribuicao = trremuneração.SelectSingleNode(".//td[9]").InnerText
                                End With
                                If trremuneração.SelectSingleNode(".//td[10]").InnerText.Trim <> "" Then
                                    For Each indicador In listaIndicadores
                                        Dim indicadoresremuneração = Split(Replace(trremuneração.SelectSingleNode(".//td[10]").InnerText.Trim, ",", ""), " ")
                                        For i = 0 To indicadoresremuneração.Length - 1
                                            If indicador.Indicador = indicadoresremuneração(i) Then
                                                If Not remuneração.Indicadores.Contains(indicador) Then remuneração.Indicadores.Add(indicador)
                                            End If
                                        Next
                                    Next
                                End If
                                If novaremuneração Then relação.Recolhimentos.Add(remuneração)
                            End If

                        Next
                    End If
                End If
                If relaçãonova = True Then
                    relação.Sequencial = vinculo.SelectSingleNode(".//table/tr[2]/td[1]").InnerText.Trim
                    If vinculo.SelectSingleNode(".//table/tr[2]/td[2]").InnerText.Trim.Length <> 10 And vinculo.SelectSingleNode(".//table/tr[2]/td[2]").InnerText.Trim.Length <> 9 Then
                        relação.CNPJ = vinculo.SelectSingleNode(".//table/tr[2]/td[2]").InnerText.Trim
                        relação.Origem = vinculo.SelectSingleNode(".//table/tr[2]/td[3]").InnerText.Trim
                        relação.Inicio = pegaData(vinculo.SelectSingleNode(".//table/tr[2]/td[4]").InnerText.Trim)
                        relação.Fim = pegaData(vinculo.SelectSingleNode(".//table/tr[2]/td[5]").InnerText.Trim)
                        relação.Filiacao = vinculo.SelectSingleNode(".//table/tr[2]/td[6]").InnerText.Trim
                        relação.UltimaRemuneracao = pegaData(vinculo.SelectSingleNode(".//table/tr[2]/td[7]").InnerText.Trim)
                        relação.Indicadores.Add(New Indicador With {.Indicador = vinculo.SelectSingleNode(".//table/tr[2]/td[1]").InnerText.Trim})
                    End If
                    Pessoa.Vinculos.Add(relação)
                End If
            Else 'Não é vínculo, é benefício
                Dim tabelasbenefício As HtmlNodeCollection = vinculo.ParentNode.ParentNode.SelectNodes(".//table")
                Dim dadosBenef1 As HtmlNode = tabelasbenefício.Item(0)
                Dim dadosbenef2 As HtmlNode
                Dim dadosbenef3 As HtmlNode

                If tabelasbenefício.Count > 2 Then
                    dadosbenef2 = tabelasbenefício.Item(1)
                    dadosbenef3 = tabelasbenefício.Item(2)
                End If
                Dim NB As String = dadosBenef1.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim
                For Each rel In Pessoa.Beneficios
                    If rel.NB = NB Then
                        benefício = rel
                        Exit For
                    End If
                Next
                If benefício Is Nothing Then
                    benefício = New Beneficio
                    benefícionovo = True
                End If
                benefício.Sequencial = dadosBenef1.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim
                benefício.NB = dadosBenef1.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim
                benefício.Especie = dadosBenef1.SelectSingleNode(".//tr[2]/td[3]").InnerText.Trim
                benefício.DER = pegaData(dadosBenef1.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim)
                benefício.DDB = pegaData(dadosBenef1.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim)

                If dadosBenef1.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim = "INDEFERIDO" Then
                    benefício.Status = dadosBenef1.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim
                    benefício.Filiacao = dadosBenef1.SelectSingleNode(".//tr[2]/td[7]").InnerText.Trim
                    benefício.RamoAtividade = dadosBenef1.SelectSingleNode(".//tr[2]/td[8]").InnerText.Trim
                    benefício.Motivo = HtmlDecode(dadosBenef1.SelectSingleNode(".//tr[2]/td[9]").InnerText.Trim)
                    benefício.APSRequerimento = dadosBenef1.SelectSingleNode(".//tr[2]/td[10]").InnerText.Trim
                Else
                    benefício.DIB = pegaData(dadosBenef1.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim)
                    benefício.DCB = pegaData(dadosBenef1.SelectSingleNode(".//tr[2]/td[7]").InnerText.Trim)
                    benefício.DIP = pegaData(dadosBenef1.SelectSingleNode(".//tr[2]/td[8]").InnerText.Trim)
                    benefício.Status = dadosBenef1.SelectSingleNode(".//tr[2]/td[9]").InnerText.Trim
                    benefício.Motivo = HtmlDecode(dadosBenef1.SelectSingleNode(".//tr[2]/td[10]").InnerText.Trim)

                    benefício.RMI = dadosbenef2.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim
                    Try
                        benefício.SB = dadosbenef2.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim
                    Catch ex As Exception
                        If ex.Message.Contains("cadeia") Then
                            benefício.SB = 0
                        End If
                    End Try
                    benefício.Coeficiente = If(SomenteNumerico(dadosbenef2.SelectSingleNode(".//tr[2]/td[3]").InnerText) <> "", SomenteNumerico(dadosbenef2.SelectSingleNode(".//tr[2]/td[3]").InnerText) / 100, 0)
                    benefício.RMA = dadosbenef2.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim
                    benefício.DAT = pegaData(dadosbenef2.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim)
                    benefício.DataNBAnterior = pegaData(dadosbenef2.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim)
                    benefício.DataObito = pegaData(dadosbenef2.SelectSingleNode(".//tr[2]/td[7]").InnerText.Trim)
                    benefício.IRT = Replace(dadosbenef2.SelectSingleNode(".//tr[2]/td[8]").InnerText, ".", ",").Trim
                    benefício.Indice1298 = dadosbenef2.SelectSingleNode(".//tr[2]/td[9]").InnerText.Trim
                    benefício.Indice0104 = dadosbenef2.SelectSingleNode(".//tr[2]/td[10]").InnerText.Trim
                    benefício.Filiacao = dadosbenef3.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim
                    benefício.RamoAtividade = dadosbenef3.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim
                    benefício.NaturezaOcupacao = dadosbenef3.SelectSingleNode(".//tr[2]/td[3]").InnerText.Trim
                    benefício.TipoConcessao = dadosbenef3.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim
                    benefício.Tratamento = dadosbenef3.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim
                    benefício.DRD = pegaData(dadosbenef3.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim)
                    benefício.APSConcessora = dadosbenef3.SelectSingleNode(".//tr[2]/td[7]").InnerText.Trim
                    benefício.APSMantenedora = dadosbenef3.SelectSingleNode(".//tr[2]/td[8]").InnerText.Trim
                End If
                If benefícionovo Then Pessoa.Beneficios.Add(benefício)
            End If
        Next
    End Sub
    Friend Sub ParseTabelaCartaConcessão(Elemento As HtmlNode, Pessoa As Pessoa)
        Dim ListaBenefícios As HtmlNodeCollection = Elemento.SelectNodes(".//div[@id='div_hiscre']")
        For Each benefHISCRE In ListaBenefícios
            Dim benefício As Beneficio
            Dim carta As New CartaConcessao
            Dim benefícionovo As Boolean = False
            If benefHISCRE.SelectSingleNode(".//center") Is Nothing Then 'Se não tem "Center", não tem carta de concessão
                Continue For
            End If
            Dim Seq As Integer = benefHISCRE.SelectSingleNode(".//center/table[1]/tr[2]/td[1]").InnerText.Trim
            For Each benef In Pessoa.Beneficios
                If benef.Sequencial = Seq Then
                    benefício = benef
                    Exit For
                End If
            Next
            If benefício Is Nothing Then
                benefício = New Beneficio
                benefícionovo = True
            End If
            Dim resumo As HtmlNode
            Dim despacho As HtmlNode
            Dim tabelaSalários As HtmlNode = benefHISCRE.SelectSingleNode(".//tr/td/table")
            If tabelaSalários Is Nothing Then
                despacho = benefHISCRE.SelectSingleNode(".//table[2]/tr[1]/td/div/center")
                resumo = Nothing
            Else
                resumo = tabelaSalários.ParentNode.ParentNode.NextSibling.NextSibling.SelectSingleNode(".//td/div/center")
                despacho = tabelaSalários.ParentNode.ParentNode.ParentNode.SelectSingleNode(".//tr[1]/td[1]/div/center")
            End If
            If resumo IsNot Nothing Then carta.Resumo = resumo.InnerText
            If despacho IsNot Nothing Then carta.Despacho = despacho.InnerText
            If tabelaSalários IsNot Nothing Then
                Dim listaSalários As HtmlNodeCollection = tabelaSalários.SelectNodes(".//tr")
                If listaSalários IsNot Nothing Then
                    For Each salário In listaSalários
                        If salário.SelectNodes(".//th") Is Nothing Then
                            If pegaData(salário.SelectSingleNode(".//td[2]").InnerText.Trim) IsNot Nothing Then
                                Dim salcon As New SalarioConcessao With {
                            .Sequencial = salário.SelectSingleNode(".//td[1]").InnerText.Trim,
                            .Competencia = pegaData(salário.SelectSingleNode(".//td[2]").InnerText.Trim),
                            .Salario = salário.SelectSingleNode(".//td[3]").InnerText.Trim,
                            .Indice = salário.SelectSingleNode(".//td[4]").InnerText.Trim,
                            .Corrigido = salário.SelectSingleNode(".//td[5]").InnerText.Trim,
                            .Observacao = salário.SelectSingleNode(".//td[6]").InnerText.Trim
                        }
                                carta.Salarios.Add(salcon)
                            End If

                            If salário.SelectSingleNode(".//td[7]") IsNot Nothing Then
                                If pegaData(salário.SelectSingleNode(".//td[8]").InnerText.Trim) IsNot Nothing Then
                                    Dim salcon As New SalarioConcessao With {
                                .Sequencial = salário.SelectSingleNode(".//td[7]").InnerText.Trim,
                                .Competencia = pegaData(salário.SelectSingleNode(".//td[8]").InnerText.Trim),
                                .Salario = salário.SelectSingleNode(".//td[9]").InnerText.Trim,
                                .Indice = salário.SelectSingleNode(".//td[10]").InnerText.Trim,
                                .Corrigido = salário.SelectSingleNode(".//td[11]").InnerText.Trim,
                               .Observacao = salário.SelectSingleNode(".//td[12]").InnerText.Trim
                            }
                                    carta.Salarios.Add(salcon)
                                End If
                            End If
                        End If
                    Next
                End If
            End If
            If benefícionovo = False Then benefício.CartaConcessao = carta
        Next
    End Sub
    Friend Sub ParseTabelaHISCRE(Elemento As HtmlNode, Pessoa As Pessoa)
        Dim ListaBenefícios As HtmlNodeCollection = Elemento.SelectNodes(".//div[@id='div_hiscre']")
        For Each benefHISCRE In ListaBenefícios
            Dim benefício As Beneficio
            If benefHISCRE.SelectSingleNode(".//center") Is Nothing Then 'Se não tem "Center", não tem HISCRE
                Continue For
            End If
            Dim benefícionovo As Boolean = False
            Dim seq As Integer = benefHISCRE.SelectSingleNode(".//center/table[1]/tr[2]/td[1]").InnerText.Trim
            For Each benef In Pessoa.Beneficios
                If benef.Sequencial = seq Then
                    benefício = benef
                    Exit For
                End If
            Next
            If benefício Is Nothing Then
                benefício = New Beneficio
                benefícionovo = True
            End If
            Dim listaCréditos As HtmlNodeCollection = benefHISCRE.SelectNodes(".//center/table[2]/tr")
            For Each linha In listaCréditos
                If linha.GetAttributeValue("style", "") = "border-bottom: 1px #ccc solid;" Then
                    Dim Histórico As New HISCRE With {
                            .Competencia = pegaData(linha.SelectSingleNode(".//td[1]").InnerText.Trim),
                            .Liquido = Regex.Replace(linha.SelectSingleNode(".//td[3]").InnerText.Trim, "[^\d]", "") / 100,
                            .Meio = linha.SelectSingleNode(".//td[4]").InnerText.Trim,
                            .Status = linha.SelectSingleNode(".//td[5]").InnerText.Trim,
                            .Previsao = pegaData(linha.SelectSingleNode(".//td[6]").InnerText.Trim),
                            .Pagamento = pegaData(linha.SelectSingleNode(".//td[7]").InnerText.Trim),
                            .Invalidado = parseBool(linha.SelectSingleNode(".//td[8]").InnerText.Trim),
                            .Isento = parseBool(linha.SelectSingleNode(".//td[9]").InnerText.Trim)
                        }
                    Dim Período = Split(linha.SelectSingleNode(".//td[2]").InnerText.Trim, " a ")
                    Histórico.Inicial = pegaData(Período(0))
                    Histórico.Final = pegaData(Período(1))
                    Dim TabelaRubricas As HtmlNode = linha.NextSibling.NextSibling.NextSibling.NextSibling.SelectSingleNode(".//td/table")
                    Dim rubricas As HtmlNodeCollection = TabelaRubricas.SelectNodes(".//tr")
                    Dim Total As Double = 0
                    Dim TotalDescontos As Double = 0
                    For Each rubrica In rubricas
                        If rubrica.SelectSingleNode(".//th") Is Nothing AndAlso rubrica.SelectSingleNode(".//td[1]").InnerText.Trim <> "" Then
                            Dim valores As New Credito With {
                                    .Rubrica = rubrica.SelectSingleNode(".//td[1]").InnerText.Trim,
                                    .Descricao = rubrica.SelectSingleNode(".//td[2]").InnerText.Trim,
                                    .Valor = Regex.Replace(rubrica.SelectSingleNode(".//td[3]").InnerText.Trim, "[^\d]", "") / 100
                                }
                            If valores.Rubrica > 100 And valores.Rubrica < 200 Then Total += valores.Valor
                            If valores.Rubrica > 200 And valores.Rubrica < 300 Then TotalDescontos += valores.Valor
                            If valores.Rubrica = 101 Then Histórico.MR = valores.Valor
                            Histórico.Creditos.Add(valores)
                        End If
                    Next
                    Histórico.Bruto = Math.Round(Total, 2)
                    Histórico.Descontos = TotalDescontos
                    benefício.HISCRE.Add(Histórico)
                    If benefícionovo Then Pessoa.Beneficios.Add(benefício)
                End If
            Next
        Next
    End Sub
    Friend Sub ParseTabelaLaudos(Elemento As HtmlNode, Pessoa As Pessoa)
        Dim tabelaPessoa As HtmlNode = Elemento.SelectSingleNode(".//table")
        Pessoa.Nome = tabelaPessoa.SelectSingleNode(".//tr[1]/td[1]").InnerText.Trim
        'If linha.SelectSingleNode(".//th").InnerText = "CPF" Then Pessoa.CPF = String.Format("{0:00000000000}", CLng(linha.SelectSingleNode(".//td").InnerText.Trim))

        If Pessoa.CPF Is Nothing Then
            If tabelaPessoa.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim <> "" Then
                Pessoa.CPF = String.Format("{0:00000000000}", CLng(tabelaPessoa.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim))
            End If
        End If

            Dim ValorNIT As String = tabelaPessoa.SelectSingleNode(".//tr[3]/td[1]").InnerText.Trim
        If ValorNIT <> "" Then
            If Not Pessoa.NIT.Contains(ValorNIT) Then
                Pessoa.NIT.Add(ValorNIT)
            End If
        End If
        Pessoa.Nascimento = tabelaPessoa.SelectSingleNode(".//tr[4]/td[1]").InnerText.Trim
        If Elemento.SelectNodes(".//table").Count < 2 Then 'Se só tem os dados básicos
            Exit Sub
        End If

        Dim ListaLaudos As HtmlNodeCollection = Elemento.SelectNodes(".//div[@class='conteudo'][1]/div[@class='conteudo']")

        For Each Laudo In ListaLaudos
            Dim NovoLaudo As New Laudo
            Dim benefício As Beneficio
            Dim benefícionovo As Boolean = False
            Dim NB As String = Laudo.SelectSingleNode(".//table[1]/tr[2]/td[2]").InnerText.Trim
            For Each benef In Pessoa.Beneficios
                If benef.NB = NB Then
                    benefício = benef
                    Exit For
                End If
            Next
            If benefício Is Nothing Then
                benefício = New Beneficio
                benefícionovo = True
            End If
            Dim tabelasLaudo As HtmlNodeCollection = Laudo.SelectNodes(".//table")
            Dim dadosLaudo As HtmlNode = tabelasLaudo.Item(0)
            Dim dadosLaudo2 As HtmlNode
            Dim dadosLaudo3 As HtmlNode

            If tabelasLaudo.Count > 2 Then
                dadosLaudo2 = tabelasLaudo.Item(1)
                dadosLaudo3 = tabelasLaudo.Item(2)
            End If

            NovoLaudo.Beneficio = dadosLaudo.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim
            NovoLaudo.NB = dadosLaudo.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim
            NovoLaudo.Requerimento = dadosLaudo.SelectSingleNode(".//tr[2]/td[3]").InnerText.Trim
            NovoLaudo.Ocupacao = dadosLaudo.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim
            NovoLaudo.DataExame = pegaData(dadosLaudo.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim)
            NovoLaudo.DER = pegaData(dadosLaudo2.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim)
            NovoLaudo.DIB = pegaData(dadosLaudo2.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim)
            NovoLaudo.DID = pegaData(dadosLaudo2.SelectSingleNode(".//tr[2]/td[3]").InnerText.Trim)
            NovoLaudo.DII = pegaData(dadosLaudo2.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim)
            NovoLaudo.DCB = pegaData(dadosLaudo2.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim)
            NovoLaudo.CID = dadosLaudo2.SelectSingleNode(".//tr[2]/td[6]").InnerText.Trim
            NovoLaudo.EncaminhaReabilitacao = parseBool(dadosLaudo3.SelectSingleNode(".//tr[2]/td[1]").InnerText.Trim)
            NovoLaudo.AcidenteTrabalho = parseBool(dadosLaudo3.SelectSingleNode(".//tr[2]/td[2]").InnerText.Trim)
            NovoLaudo.AuxilioAcidente = dadosLaudo3.SelectSingleNode(".//tr[2]/td[3]").InnerText.Trim
            NovoLaudo.IsencaoCarencia = parseBool(dadosLaudo3.SelectSingleNode(".//tr[2]/td[4]").InnerText.Trim)
            NovoLaudo.SugestaoLI = parseBool(dadosLaudo3.SelectSingleNode(".//tr[2]/td[5]").InnerText.Trim)
            NovoLaudo.Historico = Laudo.SelectSingleNode(".//p[1]").InnerText.Trim
            NovoLaudo.ExameFisico = Laudo.SelectSingleNode(".//p[2]").InnerText.Trim
            NovoLaudo.Consideracoes = Laudo.SelectSingleNode(".//p[3]").InnerText.Trim
            NovoLaudo.Resultado = Laudo.SelectSingleNode(".//p[4]").InnerText.Trim
            If benefícionovo Then
                benefício.NB = NovoLaudo.NB
                benefício.DER = NovoLaudo.DER
                benefício.DIB = NovoLaudo.DIB
                benefício.DCB = NovoLaudo.DCB
                benefício.NaturezaOcupacao = NovoLaudo.Ocupacao
            End If


            benefício.Laudos.Add(NovoLaudo)
            If benefícionovo Then Pessoa.Beneficios.Add(benefício)
            benefício = Nothing

        Next
    End Sub
    Public Function pegaData(Texto As String) As Object
        Dim dataRetorno As Date
        Try
            dataRetorno = CDate(Texto)
            Return dataRetorno
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

End Module

