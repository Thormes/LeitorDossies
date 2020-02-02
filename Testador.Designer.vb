<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Testador
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: o procedimento a seguir é exigido pelo Windows Form Designer
    'Pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.txtEntrada = New System.Windows.Forms.TextBox()
        Me.txtSaída = New System.Windows.Forms.TextBox()
        Me.lblEntrada = New System.Windows.Forms.Label()
        Me.lblSaída = New System.Windows.Forms.Label()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.LerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MédicoToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LeituraToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DossiêToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BenefíciosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VínculosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HISCREToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HISCREToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.LimparToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetarAutorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.copyClipboard = New System.Windows.Forms.Button()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtEntrada
        '
        Me.txtEntrada.Location = New System.Drawing.Point(12, 70)
        Me.txtEntrada.MaxLength = 3276745
        Me.txtEntrada.Multiline = True
        Me.txtEntrada.Name = "txtEntrada"
        Me.txtEntrada.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtEntrada.Size = New System.Drawing.Size(744, 547)
        Me.txtEntrada.TabIndex = 0
        '
        'txtSaída
        '
        Me.txtSaída.Location = New System.Drawing.Point(787, 70)
        Me.txtSaída.MaxLength = 3276745
        Me.txtSaída.Multiline = True
        Me.txtSaída.Name = "txtSaída"
        Me.txtSaída.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtSaída.Size = New System.Drawing.Size(789, 547)
        Me.txtSaída.TabIndex = 1
        '
        'lblEntrada
        '
        Me.lblEntrada.AutoSize = True
        Me.lblEntrada.Location = New System.Drawing.Point(13, 51)
        Me.lblEntrada.Name = "lblEntrada"
        Me.lblEntrada.Size = New System.Drawing.Size(44, 13)
        Me.lblEntrada.TabIndex = 2
        Me.lblEntrada.Text = "Entrada"
        '
        'lblSaída
        '
        Me.lblSaída.AutoSize = True
        Me.lblSaída.Location = New System.Drawing.Point(784, 51)
        Me.lblSaída.Name = "lblSaída"
        Me.lblSaída.Size = New System.Drawing.Size(36, 13)
        Me.lblSaída.TabIndex = 3
        Me.lblSaída.Text = "Saída"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LerToolStripMenuItem, Me.MédicoToolStripMenuItem, Me.LeituraToolStripMenuItem, Me.LimparToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(1588, 24)
        Me.MenuStrip1.TabIndex = 4
        Me.MenuStrip1.Text = "Substituições"
        '
        'LerToolStripMenuItem
        '
        Me.LerToolStripMenuItem.Name = "LerToolStripMenuItem"
        Me.LerToolStripMenuItem.Size = New System.Drawing.Size(94, 20)
        Me.LerToolStripMenuItem.Text = "Previdenciário"
        '
        'MédicoToolStripMenuItem
        '
        Me.MédicoToolStripMenuItem.Name = "MédicoToolStripMenuItem"
        Me.MédicoToolStripMenuItem.Size = New System.Drawing.Size(59, 20)
        Me.MédicoToolStripMenuItem.Text = "Médico"
        '
        'LeituraToolStripMenuItem
        '
        Me.LeituraToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DossiêToolStripMenuItem, Me.BenefíciosToolStripMenuItem, Me.VínculosToolStripMenuItem, Me.HISCREToolStripMenuItem, Me.HISCREToolStripMenuItem1})
        Me.LeituraToolStripMenuItem.Name = "LeituraToolStripMenuItem"
        Me.LeituraToolStripMenuItem.Size = New System.Drawing.Size(55, 20)
        Me.LeituraToolStripMenuItem.Text = "Leitura"
        '
        'DossiêToolStripMenuItem
        '
        Me.DossiêToolStripMenuItem.Name = "DossiêToolStripMenuItem"
        Me.DossiêToolStripMenuItem.Size = New System.Drawing.Size(178, 22)
        Me.DossiêToolStripMenuItem.Text = "Ficha Sintética"
        '
        'BenefíciosToolStripMenuItem
        '
        Me.BenefíciosToolStripMenuItem.Name = "BenefíciosToolStripMenuItem"
        Me.BenefíciosToolStripMenuItem.Size = New System.Drawing.Size(178, 22)
        Me.BenefíciosToolStripMenuItem.Text = "Benefícios"
        '
        'VínculosToolStripMenuItem
        '
        Me.VínculosToolStripMenuItem.Name = "VínculosToolStripMenuItem"
        Me.VínculosToolStripMenuItem.Size = New System.Drawing.Size(178, 22)
        Me.VínculosToolStripMenuItem.Text = "Vínculos"
        '
        'HISCREToolStripMenuItem
        '
        Me.HISCREToolStripMenuItem.Name = "HISCREToolStripMenuItem"
        Me.HISCREToolStripMenuItem.Size = New System.Drawing.Size(178, 22)
        Me.HISCREToolStripMenuItem.Text = "Carta de Concessão"
        '
        'HISCREToolStripMenuItem1
        '
        Me.HISCREToolStripMenuItem1.Name = "HISCREToolStripMenuItem1"
        Me.HISCREToolStripMenuItem1.Size = New System.Drawing.Size(178, 22)
        Me.HISCREToolStripMenuItem1.Text = "HISCRE"
        '
        'LimparToolStripMenuItem
        '
        Me.LimparToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ResetarAutorToolStripMenuItem})
        Me.LimparToolStripMenuItem.Name = "LimparToolStripMenuItem"
        Me.LimparToolStripMenuItem.Size = New System.Drawing.Size(56, 20)
        Me.LimparToolStripMenuItem.Text = "Limpar"
        '
        'ResetarAutorToolStripMenuItem
        '
        Me.ResetarAutorToolStripMenuItem.Name = "ResetarAutorToolStripMenuItem"
        Me.ResetarAutorToolStripMenuItem.Size = New System.Drawing.Size(145, 22)
        Me.ResetarAutorToolStripMenuItem.Text = "Resetar Autor"
        '
        'copyClipboard
        '
        Me.copyClipboard.Location = New System.Drawing.Point(1453, 623)
        Me.copyClipboard.Name = "copyClipboard"
        Me.copyClipboard.Size = New System.Drawing.Size(123, 23)
        Me.copyClipboard.TabIndex = 5
        Me.copyClipboard.Text = "Copiar para Clipboard"
        Me.copyClipboard.UseVisualStyleBackColor = True
        '
        'Testador
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1588, 693)
        Me.Controls.Add(Me.copyClipboard)
        Me.Controls.Add(Me.lblSaída)
        Me.Controls.Add(Me.lblEntrada)
        Me.Controls.Add(Me.txtSaída)
        Me.Controls.Add(Me.txtEntrada)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Testador"
        Me.Text = "Teste de Leitura"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtEntrada As TextBox
    Friend WithEvents txtSaída As TextBox
    Friend WithEvents lblEntrada As Label
    Friend WithEvents lblSaída As Label
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents LerToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LeituraToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DossiêToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BenefíciosToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents VínculosToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents HISCREToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LimparToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents HISCREToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents copyClipboard As Button
    Friend WithEvents MédicoToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetarAutorToolStripMenuItem As ToolStripMenuItem
End Class
