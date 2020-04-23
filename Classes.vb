Imports HtmlAgilityPack
Imports System.Text.RegularExpressions
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

            If html.Contains("<title>SAPIENS - Dossiê Previdênciário</title>") Or html.Contains("<title>SAPIENS - Extrato dos Dossiês de Defesa e Médico</title>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Previdenciario
                Mensagem = "Dossiê Previdenciário"
            ElseIf html.Contains("<b>DOSSIÊ MÉDICO</b>") Then
                Sucesso = True
                TipoDeDossie = TipoDossie.Medico
                Mensagem = "Dossiê Médico"
            Else
                Sucesso = False
                TipoDeDossie = TipoDossie.Invalido
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
                    Autor.CalculaTempoContribuicao(Autor)
                    Autor.ProcessaBeneficios()
                Catch ex As Exception
                    Sucesso = False
                Mensagem &= Environment.NewLine & "Erro ao realizar a leitura do Dossiê: " & ex.StackTrace
                End Try
            End If

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
        Private Const BenefNaoSegurado = "21,23,25,29,54,56,60,68,85,86,87,88,89,93"
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
        Public Property QualidadeDeSegurado As New List(Of QualidadeSegurado)

        Public Sub ProcessaBeneficios()
            Dim benefCessado As Beneficio
            Dim benefIndeferido As Beneficio
            Dim benefMensalidade As Beneficio
            Dim benefRequerido As Beneficio
            Dim benefAtivo As New List(Of Beneficio)
            'Autor.Beneficios.Sort(Function(x, y) CDate(x.DER).CompareTo(CDate(y.DER)))
            For Each benef In Me.Beneficios
                'If benef.Laudos.Count > 0 Then
                '    benef.Laudos.Sort(Function(x, y) CDate(x.DataExame).CompareTo(CDate(y.DataExame)))
                'End If
                calcQualidadeSegurado(benef, Me)
                If benefRequerido IsNot Nothing Then
                    If benef.DER IsNot Nothing Then
                        If benefRequerido.DER Is Nothing Then
                            benefRequerido = benef
                        Else
                            If benefRequerido.DER <= benef.DER Then benefRequerido = benef
                        End If
                    Else
                        If benef.DIB IsNot Nothing Then
                            If benefRequerido.DIB Is Nothing Then
                                benefRequerido = benef
                            Else
                                If benefRequerido.DIB <= benef.DIB Then benefRequerido = benef
                            End If
                        End If
                    End If
                Else
                    benefRequerido = benef
                End If
                If benef.DER IsNot Nothing AndAlso Nascimento IsNot Nothing Then
                    benef.IdadeNaDER = CalculaIdade(Nascimento, benef.DER)
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
                    If benef.DER IsNot Nothing Then CalculaTempoContribuicao(Me, benef)
                End If
            Next
            If benefCessado IsNot Nothing Then UltimoBeneficioCessado = benefCessado
            If benefIndeferido IsNot Nothing Then UltimoBeneficioIndeferido = benefIndeferido
            BeneficiosAtivos = benefAtivo
            If benefMensalidade IsNot Nothing Then BeneficioEmMensalidadeRecuperacao = benefMensalidade
            If benefRequerido IsNot Nothing Then UltimoBeneficioRequerido = benefRequerido
        End Sub
        Public Sub CalculaTempoContribuicao(Autor As Pessoa, Optional Beneficio As Beneficio = Nothing)
            Dim TotalDeDias As Integer
            For Each vinc In Autor.Vinculos
                If Beneficio Is Nothing Then calcQualidadeSegurado(vinc, Autor)
                Dim listaConcomitancias As New List(Of String)
                If vinc.Fim IsNot Nothing OrElse vinc.UltimaRemuneracao IsNot Nothing Then 'Se tem data final ou última remuneração
                    Dim DataInicial As Date = vinc.Inicio
                    Dim DataFinal As Date
                    If vinc.Fim Is Nothing Then
                        DataFinal = vinc.UltimaRemuneracao
                        DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                    Else
                        DataFinal = vinc.Fim
                    End If
                    If Beneficio IsNot Nothing Then
                        If Beneficio.DER Is Nothing Then Exit Sub 'Se a DER do benefício não for uma data compatível, sai do método
                        If DataInicial > Beneficio.DER Then Continue For
                        If DataFinal > Beneficio.DER Then DataFinal = Beneficio.DER
                    End If


                    For Each vinc2 In Autor.Vinculos 'Análise se o vínculo é concomitante com outros, para fins de contagem do tempo de serviço

                        If vinc.Sequencial <> vinc2.Sequencial Then 'Se não é o mesmo vínculo
                            Dim DataInicial2 As Date = vinc2.Inicio
                            Dim DataFinal2 As Date
                            If vinc2.Fim Is Nothing Then
                                If vinc2.UltimaRemuneracao Is Nothing Then
                                    DataFinal2 = DataInicial2
                                Else

                                    DataFinal2 = vinc2.UltimaRemuneracao
                                    DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                                End If
                            Else
                                DataFinal2 = vinc2.Fim
                            End If
                            If DataInicial >= DataInicial2 AndAlso DataInicial < DataFinal2 Then
                                listaConcomitancias.Add(CStr(vinc2.Sequencial))
                                If DataFinal <= DataFinal2 Then
                                    DataInicial = DataFinal
                                Else
                                    DataInicial = DataFinal2.AddDays(1)
                                End If
                            End If
                            'If DataFinal <= DataFinal2 AndAlso DataFinal > DataInicial2 Then
                            '    FinalAbsorvido = True
                            '    If DataInicial >= DataInicial2 Then
                            '        DataFinal = DataInicial
                            '    Else
                            '        DataFinal = DataInicial2.AddDays(-1)
                            '    End If
                            'End If
                        End If
                    Next

                    'Dim Dias As Integer = DateDiff(DateInterval.Day, CDate(vinc.Inicio), CDate(vinc.Fim)) - 1
                    Dim Dias As Integer
                    If DataInicial = DataFinal Then
                        Dias = 0
                    Else
                        Dias = Dias360(DataInicial, DataFinal, True) + 1
                    End If
                    Dim Anos As Integer = Math.Floor(Dias / 360)
                    Dim Meses As Integer = Math.Floor((Dias Mod 360) / 30)
                    Dim DiasRestantes As Integer = (Dias Mod 360) Mod 30
                    If Beneficio Is Nothing Then
                        vinc.Anos = Anos
                        vinc.Dias = DiasRestantes
                        vinc.Meses = Meses
                        vinc.DiasTotais = Dias
                        If listaConcomitancias.Count > 0 Then vinc.Concomitancia = String.Join(", ", listaConcomitancias)
                    End If
                    TotalDeDias += Dias
                End If
            Next
            Dim TextoAnos As String = "Anos"
            Dim TextoMeses As String = "Meses"
            Dim TextoDias As String = "Dias"
            Dim AnosTotais As Integer = Math.Floor(TotalDeDias / 360)
            Dim MesesTotais As Integer = Math.Floor((TotalDeDias Mod 360) / 30)
            Dim DiasTotaisRestantes As Integer = (TotalDeDias Mod 360) Mod 30
            If AnosTotais = 1 Then TextoAnos = "Ano"
            If MesesTotais = 1 Then TextoMeses = "Mês"
            If DiasTotaisRestantes = 1 Then TextoDias = "Dia"
            If Beneficio IsNot Nothing Then
                Beneficio.TempoAteDER = String.Format("{0} {1}, {2} {3} e {4} {5}", AnosTotais, TextoAnos, MesesTotais, TextoMeses, DiasTotaisRestantes, TextoDias)
            Else
                Autor.TempoContribuicao = String.Format("{0} {1}, {2} {3} e {4} {5}", AnosTotais, TextoAnos, MesesTotais, TextoMeses, DiasTotaisRestantes, TextoDias)
                Autor.TotalDias = TotalDeDias
            End If


        End Sub
        Public Sub calcQualidadeSegurado(Benef As Beneficio, Pessoa As Pessoa)

            If Benef.DIB Is Nothing Then Exit Sub
            If Benef.Especie Is Nothing Then Exit Sub

            Dim codEspecie As String = Regex.Replace(Benef.Especie, "[^\d]", "")
            If Not BenefNaoSegurado.Contains(codEspecie) Then 'Se é espécie de benefício que garante qualidade de segurado
                If Benef.DIB IsNot Nothing Then
                    If Pessoa.QualidadeDeSegurado.Count = 0 Then
                        Dim novaQualidade As New QualidadeSegurado
                        novaQualidade.Inicio = Benef.DIB
                        novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Beneficio
                        If Benef.DCB IsNot Nothing Then
                            Dim dataFim As Date = DateAdd("m", 14, Benef.DCB)
                            dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                            novaQualidade.Fim = dataFim
                        Else
                            novaQualidade.Fim = Today
                        End If
                        Pessoa.QualidadeDeSegurado.Add(novaQualidade)
                    Else
                        Dim novaQualidade As QualidadeSegurado
                        Dim semQualidadeAnterior As Boolean = True
                        For Each Periodo In Pessoa.QualidadeDeSegurado
                            If Benef.DIB >= Periodo.Inicio And Benef.DIB <= Periodo.Fim Then 'Se a DIB está dentro do período da qualidade de segurado
                                novaQualidade = Periodo
                                semQualidadeAnterior = False
                            End If
                        Next
                        If semQualidadeAnterior Then novaQualidade = New QualidadeSegurado With {.tipodeQualidade = QualidadeSegurado.tipoQualidade.Beneficio}

                        If semQualidadeAnterior Then novaQualidade.Inicio = Benef.DIB
                        If Benef.DCB IsNot Nothing Then
                            Dim dataFim As Date = DateAdd("m", 14, Benef.DCB)
                            dataFim = DateSerial(Year(dataFim), Month(dataFim), 15)
                            If novaQualidade.Fim Is Nothing OrElse novaQualidade.Fim < dataFim Then
                                novaQualidade.Fim = dataFim
                                novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Beneficio
                            End If
                        Else
                            novaQualidade.Fim = Today
                        End If
                        If semQualidadeAnterior Then Pessoa.QualidadeDeSegurado.Add(novaQualidade)
                    End If
                End If
            End If
        End Sub
        Public Sub calcQualidadeSegurado(vinc As Vinculo, Pessoa As Pessoa)

            If vinc.Fim Is Nothing AndAlso vinc.UltimaRemuneracao Is Nothing Then Exit Sub

            If vinc.Inicio IsNot Nothing Then
                If Pessoa.QualidadeDeSegurado.Count = 0 Then
                    Dim novaQualidade As New QualidadeSegurado
                    novaQualidade.Inicio = vinc.Inicio
                    novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Atividade

                    If vinc.Fim IsNot Nothing Then
                        Dim dataFim As Date = DateAdd("m", 14, vinc.Fim)
                        dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                        novaQualidade.Fim = dataFim
                    Else
                        If vinc.UltimaRemuneracao IsNot Nothing Then
                            Dim dataFim As Date
                            dataFim = vinc.UltimaRemuneracao
                            dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                            dataFim = DateAdd("m", 14, dataFim)
                            dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                            novaQualidade.Fim = dataFim
                        End If
                    End If
                    Pessoa.QualidadeDeSegurado.Add(novaQualidade)
                Else
                    Dim novaQualidade As QualidadeSegurado
                    Dim semQualidadeAnterior As Boolean = True
                    For Each Periodo In Pessoa.QualidadeDeSegurado
                        If vinc.Inicio >= Periodo.Inicio And vinc.Inicio <= Periodo.Fim Then 'Se o início do vínculo está dentro do período da qualidade de segurado
                            novaQualidade = Periodo
                            novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Atividade
                            semQualidadeAnterior = False
                        End If
                    Next
                    If semQualidadeAnterior Then novaQualidade = New QualidadeSegurado With {.tipodeQualidade = QualidadeSegurado.tipoQualidade.Atividade}

                    If semQualidadeAnterior Then novaQualidade.Inicio = vinc.Inicio
                    If vinc.Fim IsNot Nothing Then
                        Dim dataFim As Date = DateAdd("m", 14, vinc.Fim)
                        dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                        If novaQualidade.Fim Is Nothing OrElse novaQualidade.Fim < dataFim Then
                            novaQualidade.Fim = dataFim
                            novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Atividade
                        End If
                    Else
                        If vinc.UltimaRemuneracao IsNot Nothing Then
                            Dim dataFim As Date
                            dataFim = vinc.UltimaRemuneracao
                            dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                            dataFim = DateAdd("m", 14, dataFim)
                            dataFim = DateSerial(dataFim.Year, dataFim.Month, 15)
                            If novaQualidade.Fim Is Nothing OrElse novaQualidade.Fim < dataFim Then
                                novaQualidade.Fim = dataFim
                                novaQualidade.tipodeQualidade = QualidadeSegurado.tipoQualidade.Atividade
                            End If
                        End If
                    End If
                    If semQualidadeAnterior Then Pessoa.QualidadeDeSegurado.Add(novaQualidade)
                End If
            End If

        End Sub
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