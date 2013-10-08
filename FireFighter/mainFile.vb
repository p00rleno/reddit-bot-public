Imports com.reddit.api
Imports com.reddit
Imports System.Text.RegularExpressions

Module mainFile
    'Variables
    Dim interval As Integer
    Dim blacklist As New ArrayList()

    Dim searchContents, markSpam, giveReason As Boolean
    Dim targetSubreddit, responseText, username, password As String
    Dim loginSession As Session

    Dim userPostHistory As New Dictionary(Of String, ArrayList)
    Dim postLimitTimer As Integer
    Dim postLimitCount As Integer


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
            Console.WriteLine("Login OK")
            While True
                Console.WriteLine("Loading /about/unmoderated...")
                Dim unmoderated As PostListing = api.Sub.getUnmoderated(loginSession, targetSubreddit)

                If Not unmoderated.ModHash.Length > 2 Then
                    Throw New Exception("It doesn't appear you have moderator permissions in the specified subreddit!")
                End If
#If CONFIG = "regex" Then
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
#ElseIf CONFIG = "frequency" Then
                Dim lastCounted As DateTime = Date.UtcNow.AddMinutes(-1 * postLimitTimer)
                unmoderated.Reverse()
                For Each p As Post In unmoderated
                    If p.CreatedUtc > lastCounted And Not checkPostAlreadySeen(p) Then
                        Console.WriteLine("Checking new post {0} by {1}...", p.Title, p.Author)
                        Dim userList As New ArrayList
                        If userPostHistory.TryGetValue(p.Author, userList) Then
                            Dim i As Integer = 0
                            Dim toRemove As New ArrayList
                            For Each oldPost As Post In userList
                                If oldPost.CreatedUtc < lastCounted Then
                                    toRemove.Add(oldPost)
                                Else
                                    i += 1
                                End If
                            Next
                            For Each a In toRemove
                                userList.Remove(a)
                            Next
                            If i >= postLimitCount Then
                                Console.WriteLine("...Post is NOT OK.")
                                Post.Remove(loginSession, targetSubreddit, p.Name, unmoderated.ModHash, markSpam)
                                If giveReason Then
                                    Comment.Submit(loginSession, p.Name, responseText)
                                    Console.WriteLine(" ...reply posted")
                                End If
                                GoTo nextloop
                            Else
                                Console.WriteLine("...Post is OK.")
                            End If

                            userPostHistory.Remove(p.Author)
                        Else
                            userList = New ArrayList
                            Console.WriteLine("...Post is OK.")
                        End If
                        userList.Add(p)
                        userPostHistory.Add(p.Author, userList)
nextloop:
                    End If
                Next
#End If

                Console.WriteLine("End of queue. Sleeping for {0} seconds.", interval / 1000)
                Console.WriteLine()
                Threading.Thread.Sleep(interval)
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

        postLimitTimer = My.Settings.postLimitTimer
        postLimitCount = My.Settings.postLimitCount

        If giveReason Then
            Try
                responseText = My.Computer.FileSystem.ReadAllText(My.Settings.replyMarkupFile)
            Catch ex As Exception
                Console.WriteLine("I couldn't find your autoreply markdown file! Either disable auto reply, create the missing file, or point the setting in app.config to the right location")
                Console.ReadKey()
                Return False
            End Try
        End If

#If CONFIG = "regex" Then
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
#End If
        Return True
    End Function

    Private Function checkPostAlreadySeen(ByVal id As Post)
        For Each postList As ArrayList In userPostHistory.Values()
            If postList.Contains(id) Then
                Return True
            End If
        Next
        Return False
    End Function

End Module
