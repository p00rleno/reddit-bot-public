Imports com.reddit.api
Imports com.reddit
Module program
    Public users As New Hashtable

    Dim subreddit As String = "leagueoflegends"
    Dim username, password As String
    Dim activeThreads As New ArrayList
    Sub Main()
        Console.WriteLine("Input Login Username:")
        username = Console.ReadLine().Trim()

        Console.WriteLine("Input Login Password:")
        Dim info As ConsoleKeyInfo
        Do
            info = Console.ReadKey(True)
            If info.Key = ConsoleKey.Enter Then
                Console.WriteLine()
                Exit Do
            ElseIf info.Key = ConsoleKey.Backspace Then
                If password.Length > 0 Then
                    password = password.Remove(password.Length - 1, 1)
                    Console.Write(vbBack)
                    Console.Write(" ")
                    Console.Write(vbBack)
                End If
                ElseIf info.Key = ConsoleKey.Escape Then
                    Exit Sub
                Else
                    password &= info.KeyChar
                    Console.Write("*"c)
                End If
        Loop

        Dim s As Session
        Try
            Console.WriteLine("Logging in...")
            s = User.Login(username, password)
            Console.WriteLine("Login OK")
            Console.WriteLine()
        Catch
            Console.WriteLine("Eek! That login ain't right! I'ma give up and quit now.")
            Console.ReadKey()
            Exit Sub
        End Try

        While (True)
            Console.WriteLine("Loading /r/{0}/new...", subreddit)
            Dim list As PostListing = [Sub].GetListing(s, subreddit, SubSortBy.New)
            Dim i As Integer = 0
            While (list(i).CreatedUtc > Date.UtcNow.AddMinutes(-5))
                i += 1
            End While
            i -= 1

            Console.WriteLine("Clearing recent queue, {0} items", i + 1)
            While Not i = -1
                Do Until list(i).CreatedUtc < Date.UtcNow.AddMinutes(-5)
                    Threading.Thread.Sleep(50)
                Loop
                Console.WriteLine("Checking thread {0}", list(i).Title)
                checkThread(list(i), s)
                i -= 1
            End While
            Console.WriteLine("Queue cleared!")
            Console.WriteLine("Saving results...")
            Console.WriteLine(mapToString())
            Console.WriteLine()
        End While
    End Sub

    Private Sub checkThread(ByVal p As Post, ByVal s As Session)
            Dim cl As CommentListing = Post.GetComments(s, p.Name)
            For Each c As Comment In cl
                If Not c.Author = p.Author Then
                    If users.ContainsKey(c.Author) Then
                        users(c.Author) += 1
                    Else
                        users(c.Author) = 1
                    End If
                End If
            Next
    End Sub

    Private Function mapToString() As String
        mapToString = ""
        For Each k As String In users.Keys
            mapToString = users(k) & vbTab & "/u/" & k & vbCr
        Next
    End Function

End Module
