Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices
Public Class Form1
    Public Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Int16

    Dim LastID As String = ""
    Dim EnterLock As Boolean = False
    Dim MouseLock As Boolean = False
    Dim HasPastedSomething As Boolean = False
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
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
        If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
            My.Computer.Clipboard.SetText(TextBox1.Text.Replace("<id>", RichTextBox1.Lines(0)))
            RichTextBox1.SelectionStart = RichTextBox1.GetFirstCharIndexFromLine(0)
            RichTextBox1.SelectionLength = RichTextBox1.Lines(0).Length + 1
            RichTextBox1.SelectedText = String.Empty
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles ColorDecayTimer.Tick
        ColorDecayTimer.Stop()
        Button1.BackColor = Color.LightGray
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "\command.txt") Then
            TextBox1.Text = My.Computer.FileSystem.ReadAllText(Application.StartupPath & "\command.txt")
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
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

        If GetAsyncKeyState(Keys.Enter) = -32767 And EnterLock = False Then
            If GetActiveProcess.ProcessName = ComboBox1.Text Or CheckBox4.Checked = False Then
                EnterLock = True
                System.Threading.Thread.Sleep(64)
                If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
                    SendKeys.SendWait(TextBox1.Text.Replace("<id>", RichTextBox1.Lines(0)))
                    RichTextBox1.SelectionStart = RichTextBox1.GetFirstCharIndexFromLine(0)
                    RichTextBox1.SelectionLength = RichTextBox1.Lines(0).Length + 1
                    RichTextBox1.SelectedText = String.Empty
                End If

                System.Threading.Thread.Sleep(64)
                EnterLock = False
            End If
        End If

        If GetAsyncKeyState(Keys.LButton) = -32767 And CheckBox3.Checked = True And MouseLock = False Then
            If GetActiveProcess.ProcessName = ComboBox1.Text Or CheckBox4.Checked = False Then
                MouseLock = True
                System.Threading.Thread.Sleep(64)
                If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Or HasPastedSomething = True Then
                    SendKeys.Send("{TAB}")
                End If

                While GetAsyncKeyState(Keys.LButton) = -32767
                    'Do not resume code until mouse has been released.
                End While
                System.Threading.Thread.Sleep(64)


                If True = True Then
                    If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
                        System.Threading.Thread.Sleep(64)
                        SendKeys.Send("{ENTER}")

                        System.Threading.Thread.Sleep(128)
                        If RichTextBox1.Lines.Count >= 0 And RichTextBox1.Text <> "" Then
                            SendKeys.SendWait(TextBox1.Text.Replace("<id>", RichTextBox1.Lines(0)))
                            RichTextBox1.SelectionStart = RichTextBox1.GetFirstCharIndexFromLine(0)
                            RichTextBox1.SelectionLength = RichTextBox1.Lines(0).Length + 1
                            RichTextBox1.SelectedText = String.Empty
                        End If
                        HasPastedSomething = True
                    ElseIf HasPastedSomething = True Then
                        HasPastedSomething = False
                        System.Threading.Thread.Sleep(64)
                        SendKeys.Send("{ENTER}")

                        System.Threading.Thread.Sleep(128)

                    End If
                End If
            End If


            MouseLock = False
        End If
    End Sub
    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            'LastID = My.Computer.Clipboard.GetText
            Clipboardobserver.Start()
            CheckBox1.CheckState = False
        Else
            Clipboardobserver.Stop()
        End If
    End Sub

    Private Sub Clipboardobserver_Tick(sender As Object, e As EventArgs) Handles Clipboardobserver.Tick
        Dim ClipBoardCache As String = My.Computer.Clipboard.GetText
        If ClipBoardCache.Length >= MinTextBox.Text And ClipBoardCache.Length <= MaxTextBox.Text Then
            If Regex.IsMatch(ClipBoardCache, "^[0-9 ]+$") Then
                If ClipBoardCache = LastID = False Then
                    If RichTextBox1.Text.Contains(ClipBoardCache) = False Then
                        Button1.BackColor = Color.Green
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