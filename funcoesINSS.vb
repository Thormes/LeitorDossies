Imports LeitorDossies.Minerador
Module funcoesINSS

    ''' <summary>
    ''' Calcula a quantidade de dias entre duas datas,
    ''' </summary>
    ''' <param name="Inicio"></param>
    ''' <param name="Fim"></param>
    ''' <param name="Europeu"></param>
    ''' <returns></returns>
    Friend Function Dias360(Inicio As DateTime, Fim As DateTime, Europeu As Boolean) As Integer
        Dim fim1 As DateTime

        If Fim < Inicio Then
            fim1 = Inicio
            Inicio = Fim
            Fim = fim1
        End If

        Dim meses As Integer = (Fim.Year - Inicio.Year) * 12 + Fim.Month - Inicio.Month

        If Europeu Then
            'Usar método europeu (meses de apenas 30 dias)
            Return meses * 30 + Math.Min(30, Fim.Day) - Math.Min(30, Inicio.Day)

        Else 'Usar método americano
            'Se o dia inicial é o último dia do mês, muda para o dia 30
            Dim DiaInicio As Integer = Inicio.Day
            DiaInicio = If(Inicio.Day >= DateTime.DaysInMonth(Inicio.Year, Inicio.Month), 30, Inicio.Day)

            'Se o dia final é o último dia do mês, muda para o dia 30
            Dim DiaFim As Integer = Fim.Day
            DiaFim = If(Fim.Day >= DateTime.DaysInMonth(Fim.Year, Fim.Month), 30, Fim.Day)

            'Se o dia final é o último do mês e o inicial é anterior ao 30º, muda o dia final para o dia 1º do mês seguinte
            If Fim.Day >= DateTime.DaysInMonth(Fim.Year, Fim.Month) And DiaInicio < 30 Then
                DiaFim = 1
                meses += 1
            End If

            Return meses * 30 + DiaFim - DiaInicio
        End If
    End Function
    Friend Function DiasCoincidentes(ByVal DataInicial As DateTime, ByVal DataFinal As DateTime, ByVal DataInicial2 As DateTime, ByVal DataFinal2 As DateTime) As Integer
        Dim MaximoInicio As DateTime = If(DataInicial > DataInicial2, DataInicial, DataInicial2)
        Dim MinimoFinal As DateTime = If(DataFinal < DataFinal2, DataFinal, DataFinal2)

        Dim retorno As Integer = If(MaximoInicio < MinimoFinal, Dias360(MaximoInicio, MinimoFinal, True), 0)
        Return retorno
    End Function
    ''' <summary>
    ''' Método para cálculo do tempo de contribuição do Autor. Se informado um benefício, o cálculo do tempo de contribuição será calculado apenas até a DER do benefício.
    ''' Se não informado um benefício, será calculado todo o tempo de contribuição da pessoa informada, registrando em cada vínculo o seu respectivo tempo de contribuição calculado
    ''' Quando há vínculos concomitantes, o início do vínculo posterior é postergado para a data imediatamente posterior ao final do vínculo anterior
    ''' </summary>
    ''' <param name="Autor"></param>
    ''' <param name="Beneficio"></param>
    Friend Sub CalculaTempoContribuicao(Autor As Pessoa, Optional Beneficio As Beneficio = Nothing)
        Dim TotalDeDias As Integer
        For Each vinc In Autor.Vinculos
            Dim listaConcomitancias As New List(Of String)
            If vinc.Fim <> "" OrElse vinc.UltimaRemuneracao <> "" Then 'Se tem data final ou última remuneração
                Dim DataInicial As Date = CDate(vinc.Inicio)
                Dim DataFinal As Date
                If vinc.Fim = "" Then
                    DataFinal = CDate(vinc.UltimaRemuneracao)
                    DataFinal = DateSerial(DataFinal.Year, DataFinal.Month, DateTime.DaysInMonth(DataFinal.Year, DataFinal.Month))
                Else
                    DataFinal = CDate(vinc.Fim)
                End If
                If Beneficio IsNot Nothing Then
                    If Beneficio.DER.Length <> 10 Then Exit Sub 'Se a DER do benefício não for uma data compatível, sai do método
                    If DataInicial > CDate(Beneficio.DER) Then Continue For
                    If DataFinal > CDate(Beneficio.DER) Then DataFinal = CDate(Beneficio.DER)
                End If


                For Each vinc2 In Autor.Vinculos 'Análise se o vínculo é concomitante com outros, para fins de contagem do tempo de serviço

                    If vinc.Sequencial <> vinc2.Sequencial Then 'Se não é o mesmo vínculo
                        Dim DataInicial2 As Date = CDate(vinc2.Inicio)
                        Dim DataFinal2 As Date
                        If vinc2.Fim = "" Then
                            If vinc2.UltimaRemuneracao = "" Then
                                DataFinal2 = DataInicial2
                            Else

                                DataFinal2 = CDate(vinc2.UltimaRemuneracao)
                                DataFinal2 = DateSerial(DataFinal2.Year, DataFinal2.Month, DateTime.DaysInMonth(DataFinal2.Year, DataFinal2.Month))
                            End If
                        Else
                            DataFinal2 = CDate(vinc2.Fim)
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
        End If


    End Sub
    Friend Function CalculaIdade(Nascimento As Date, Final As Date) As String
        If Nascimento > Final Then
            Return "Data de Nascimento Maior que data a calcular"
        End If

        Dim DifAno As Integer
        Dim DifMes As Integer
        Dim DifDia As Integer
        Dim AnoDN As Integer = Nascimento.Year
        Dim MesDN As Integer = Nascimento.Month
        Dim DiaDN As Integer = Nascimento.Day
        Dim AnoFim As Integer = Final.Year
        Dim MesFim As Integer = Final.Month
        Dim DiaFim As Integer = Final.Day
        Dim Idade As String
        DifAno = AnoFim - AnoDN
        DifMes = MesFim - MesDN
        DifDia = DiaFim - DiaDN
        Dim TextoAnos As String = "Anos"
        Dim TextoMeses As String = "Meses"
        Dim TextoDias As String = "Dias"
        If (DifMes) < 0 Then
            DifAno -= 1
        End If
        If (DifDia) < 0 Then
            DifMes -= 1
            If (MesFim - 1) < 8 Then
                If ((MesFim - 1) Mod 2) = 0 Then
                    If (MesFim - 1) = 2 Then
                        If AnoFim Mod 4 = 0 Then
                            DifDia = 29 + DifDia
                        Else
                            DifDia = 28 + DifDia
                        End If
                    Else
                        DifDia = 30 + DifDia
                    End If
                Else
                    DifDia = 31 + DifDia
                End If
            Else
                If ((MesFim - 1) Mod 2) = 0 Then
                    DifDia = 31 + DifDia
                Else
                    DifDia = 30 + DifDia
                End If
            End If
        End If
        If (DifMes) < 0 Then
            DifMes = DifMes + 12
        End If

        If DifAno < 0 Then
            DifAno = DifAno * (-1)
        End If

        If (DifDia) < 0 Then
            DifDia = DifDia * (-1)
        End If

        If DifAno = 1 Then TextoAnos = "Ano"
        If DifMes = 1 Then TextoMeses = "Mês"
        If DifDia = 1 Then TextoDias = "Dia"


        Idade = String.Format("{0} {1}, {2} {3} e {4} {5}", DifAno.ToString(), TextoAnos, DifMes.ToString(), TextoMeses, DifDia.ToString(), TextoDias)
        Return Idade

    End Function
End Module
