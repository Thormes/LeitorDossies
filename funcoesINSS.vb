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
End Module
