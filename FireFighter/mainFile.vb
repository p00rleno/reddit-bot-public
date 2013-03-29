Imports com.reddit.api
Imports com.reddit
Imports System.Text.RegularExpressions

Module mainFile
    'Variables
    Dim searchContents, markSpam, giveReason As Boolean
    Dim targetSubreddit, responseText, username, password As String
    Dim interval As Integer
    Dim blacklist As New ArrayList()
    Dim loginSession As Session
    Sub Main()
        Console.Title = "p00rleno's Reddit FireFighter"
        If Not loadVariables() Then Exit Sub
        'Start application loop
        Try
            Console.WriteLine("Logging in as {0}...", username)
            Try
                loginSession = User.Login(username, password)
            Catch ex As Exception
                Throw New Exception("I couldn't login with the credentials you specified. Fix them in the config file then retry")
            End Try

            While True

                Dim unmoderated As PostListing = api.Sub.getUnmoderated(loginSession, targetSubreddit)

                If Not unmoderated.ModHash.Length > 2 Then
                    Throw New Exception("It doesn't appear you have moderator permissions in the specified subreddit!")
                End If

                For Each p As Post In unmoderated
                    For Each bannedRegex As String In blacklist
                        If Regex.Match(p.Title, bannedRegex).Success Or (searchContents And Regex.Match(p.SelfText, bannedRegex).Success) Then
                            Post.Remove(loginSession, targetSubreddit, p.Name, unmoderated.ModHash, markSpam)
                            Console.WriteLine(String.Format("{0} : Thread {1} removed.", Now.ToShortTimeString, p.Title))
                            If giveReason Then
                                Comment.Submit(loginSession, p.Name, responseText)
                                Console.WriteLine(" ...reply posted")
                            End If
                        End If
                    Next
                Next

                Threading.Thread.Sleep(1000 * interval)
            End While
        Catch ex As Exception
            Console.WriteLine("The program has crashed. The exception was:")
            Console.WriteLine(ex.Message)
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey()
            Exit Sub
        End Try
    End Sub

    Private Function loadVariables()
        searchContents = My.Settings.searchSelfText
        markSpam = My.Settings.markAsSpam
        giveReason = My.Settings.replyWithComment

        interval = 1000 * My.Settings.sleepDuration

        targetSubreddit = My.Settings.targetSubreddit
        username = My.Settings.username
        password = My.Settings.password

        If giveReason Then
            Try
                responseText = My.Computer.FileSystem.ReadAllText(My.Settings.replyMarkupFile)
            Catch ex As Exception
                Console.WriteLine("I couldn't find your autoreply markdown file! Either disable auto reply, create the missing file, or point the setting in app.config to the right location")
                Console.ReadKey()
                Return False
            End Try
        End If

        Try
            Dim regexFile As String = My.Computer.FileSystem.ReadAllText(My.Settings.regexListFile)
            For Each line As String In regexFile.Split(vbNewLine)
                If Not line.StartsWith("#") Then
                    blacklist.Add(line.Trim)
                End If
            Next
        Catch ex As Exception
            Console.WriteLine("I couldn't find the regex file! Either make it or point app.config in the right direction...")
            Console.ReadKey()
            Return False
        End Try

        Return True
    End Function

End Module
