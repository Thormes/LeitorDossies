Imports LeitorDossies.Minerador
Imports System.Text.RegularExpressions

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
