Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices
Public Class Form1
    Public Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Int16

    Dim LastID As String = ""
    Dim EnterLock As Boolean = False
    Dim MouseLock As Boolean = False
    Dim HasPastedSomething As Boolean = False
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'Copy text from clipboard into RichTextBox1 and advance it accordingly.
        If RichTextBox1.Text.Contains(My.Computer.Clipboard.GetText) = True Then
            Button1.BackColor = Color.Red
            ColorDecayTimer.Start()
        Else
            Button1.BackColor = Color.Green
            ColorDecayTimer.Start()
            If RichTextBox1.Text = "" Then
                RichTextBox1.Text = My.Computer.Clipboard.GetText
            Else
                RichTextBox1.Text = RichTextBox1.Text & vbNewLine & My.Computer.Clipboard.GetText
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Call PasteNextComand with disabled send mode.
        PasteNextCommand(False)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles ColorDecayTimer.Tick
        'This get's started when the button color was changed to revert back to default.
        ColorDecayTimer.Stop()
        Button1.BackColor = Color.LightGray
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Check if command.txt existst and load it's contents into the command field.
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "\command.txt") Then
            TextBox1.Text = My.Computer.FileSystem.ReadAllText(Application.StartupPath & "\command.txt")
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        'Checks and starts/stopps key monitoring timer depending on selected mode.
        If CheckBox1.Checked = True Then
            Keyobserver.Start()
            CheckBox2.CheckState = False
            CheckBox3.Enabled = True
        Else
            Keyobserver.Stop()
            CheckBox3.Enabled = False
        End If
    End Sub

    Private Sub Keyobserver_Tick(sender As Object, e As EventArgs) Handles Keyobserver.Tick

        'If Auto Paste is enabled this waits for ENTER to be pressed to type in the next ID / Command
        If GetAsyncKeyState(Keys.Enter) = -32767 And EnterLock = False Then
            If GetActiveProcess.ProcessName = ComboBox1.Text Or CheckBox4.Checked = False Then
                EnterLock = True
                System.Threading.Thread.Sleep(64)
                'Call PasteNextComand with enabled send mode.
                PasteNextCommand(True)
                System.Threading.Thread.Sleep(64)
                EnterLock = False
            End If
        End If

        'If Auto+ is enabled, waits for the LMouse button to be pressed.
        If GetAsyncKeyState(Keys.LButton) = -32767 And CheckBox3.Checked = True And MouseLock = False Then
            'If Appcheck is enabled make sure the current window to paste into is the whitelisted window.
            If GetActiveProcess.ProcessName = ComboBox1.Text Or CheckBox4.Checked = False Then
                MouseLock = True
                System.Threading.Thread.Sleep(64)

                'Tap into text box to prepare typing in the command.
                If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Or HasPastedSomething = True Then
                    SendKeys.Send("{TAB}")
                End If

                While GetAsyncKeyState(Keys.LButton) = -32767
                    'Do not resume code until mouse has been released.
                End While
                System.Threading.Thread.Sleep(64)

                'If RichTextbox1 is not empty, paste the next command and send it with the ENTER key.
                If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
                    System.Threading.Thread.Sleep(64)
                    SendKeys.Send("{ENTER}")
                    System.Threading.Thread.Sleep(128)
                    'Call PasteNextComand with enabled send mode.
                    PasteNextCommand(True)
                    HasPastedSomething = True

                    'If RichTextbox1 has just become empty, send a last ENTER command to send the text that should still be present in the windows chat line.
                ElseIf HasPastedSomething = True Then
                    HasPastedSomething = False
                    System.Threading.Thread.Sleep(64)
                    SendKeys.Send("{ENTER}")
                    System.Threading.Thread.Sleep(128)
                End If
            End If
            MouseLock = False
        End If
    End Sub
    Private Function PasteNextCommand(PasteMode As Boolean)
        'This function handled the loading and pasting or copying of the next command, then removes the current first line from the RichTextbox1 and advances it accordingly
        If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
            If PasteMode = True Then
                If TextBox1.Text = "" Then
                    SendKeys.SendWait(RichTextBox1.Lines(0))
                Else
                    SendKeys.SendWait(TextBox1.Text.Replace("<id>", RichTextBox1.Lines(0)))
                End If
            Else
                If TextBox1.Text = "" Then
                    Clipboard.SetText(RichTextBox1.Lines(0))
                Else
                    Clipboard.SetText(TextBox1.Text.Replace("<id>", RichTextBox1.Lines(0)))
                End If
            End If
            RichTextBox1.SelectionStart = RichTextBox1.GetFirstCharIndexFromLine(0)
            RichTextBox1.SelectionLength = RichTextBox1.Lines(0).Length + 1
            RichTextBox1.SelectedText = String.Empty
        End If
    End Function
    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        'Handles starting or stopping of the clipboard detection function.
        If CheckBox2.Checked = True Then
            'LastID = My.Computer.Clipboard.GetText
            Clipboardobserver.Start()
            CheckBox1.CheckState = False
        Else
            Clipboardobserver.Stop()
        End If
    End Sub

    Private Sub Clipboardobserver_Tick(sender As Object, e As EventArgs) Handles Clipboardobserver.Tick
        'This function monitors the clipboard for ID's being copied, if AutoADD is enabled and thus this timer is running.
        Dim ClipBoardCache As String = My.Computer.Clipboard.GetText
        'Check if the lenght is within specified MIN/MAX detection range.
        If ClipBoardCache.Length >= MinTextBox.Text And ClipBoardCache.Length <= MaxTextBox.Text Then
            'Check that it's only numbers.
            If Regex.IsMatch(ClipBoardCache, "^[0-9 ]+$") Then
                'Handle adding and give a feedback to the user by the copy button green.
                If ClipBoardCache = LastID = False Then
                    If RichTextBox1.Text.Contains(ClipBoardCache) = False Then
                        Button1.BackColor = Color.Green
                        'Start delay timer to make the button color normal again.
                        ColorDecayTimer.Start()
                        If RichTextBox1.Text = "" Then
                            RichTextBox1.Text = ClipBoardCache
                            LastID = ClipBoardCache
                        Else
                            RichTextBox1.Text = RichTextBox1.Text & vbNewLine & ClipBoardCache
                        End If
                    End If
                End If
            End If
        End If
    End Sub
    Public Function GetActiveProcess() As Process
        Dim hWnd As IntPtr = NativeMethods.GetForegroundWindow()
        Dim ProcessID As UInteger = 0

        NativeMethods.GetWindowThreadProcessId(hWnd, ProcessID)

        Return If(ProcessID <> 0, Process.GetProcessById(ProcessID), Nothing)
    End Function

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged
        'Update line conter label if text changed.
        Label2.Text = RichTextBox1.Lines.Length
    End Sub
End Class
Public NotInheritable Class NativeMethods
        Private Sub New() 'Private constructor as we're not supposed to create instances of this class.
        End Sub

        <DllImport("user32.dll")>
        Public Shared Function GetForegroundWindow() As IntPtr
        End Function

        <DllImport("user32.dll")>
        Public Shared Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, <Out()> ByRef lpdwProcessId As UInteger) As UInteger
        End Function
    End Class