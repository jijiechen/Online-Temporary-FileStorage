Imports System.IO
Imports System.Collections.Generic

Partial Class destination
    Inherits System.Web.UI.Page

    Private Const FilePath As String = "~/files/"
    Public FileListStr As String = String.Empty

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim files As HttpFileCollection = Request.Files
        Dim delFie As String = Request.QueryString("del")
        Dim getFile As String = Request.QueryString("get")

        If (Request.HttpMethod = "POST") AndAlso _
            (Not (files Is Nothing)) AndAlso _
            (files.Count > 0) Then
            SaveRequestedFile()
            If Request.QueryString("ajax") Is Nothing Then
                Response.Redirect("Destination.aspx")
            End If
        Else
            If (Not (delFie Is Nothing)) AndAlso delFie.Length > 0 Then
                DeleteFile(delFie)
                Response.Redirect("Destination.aspx")
                Return
            End If
            If (Not (getFile Is Nothing)) AndAlso getFile.Length > 0 Then
                DownloadFile(getFile)
                Return
            End If
        End If

        If (Request.QueryString("ajax") Is Nothing) Then
            WriteFileList()
        End If

    End Sub

    Private Sub SaveRequestedFile()
        Dim files As HttpFileCollection = Request.Files

        Dim fileName As String = Guid.NewGuid().ToString("N")       '取新文件名种子
        Dim file As HttpPostedFile = files(0)                       '获取要转存的文件
        Dim fileConfigName As String         '定义新的文件名

        fileConfigName = String.Concat(fileName, ".config")

        file.SaveAs(Server.MapPath(String.Concat(FilePath, fileName)))
        Dim configFile As StreamWriter = System.IO.File.CreateText(Server.MapPath(String.Concat(FilePath, "xml/", fileConfigName)))
        configFile.Write(file.FileName)
        configFile.Close()

    End Sub

    Private Class FileInfoCompare
        Implements IComparer(Of FileInfo)

        Public Function Compare(ByVal x As System.IO.FileInfo, ByVal y As System.IO.FileInfo) As Integer Implements System.Collections.Generic.IComparer(Of System.IO.FileInfo).Compare
            Return x.CreationTime - y.CreationTime
        End Function
    End Class

    Private Sub WriteFileList()
        Dim FileNameResult As System.Text.StringBuilder, FileNameFormat As String = "<tr><td style=""min-width:360px;_width:360px;_overflow-x:visible""><a title=""{1}"" href=""Destination.aspx?get={0}"" target=""_blank"">{1}</a></td><td style=""width:160px"">{2}</td><td style=""width:80px""><a href=""Destination.aspx?del={0}"">Delete</a></td></tr>"
        FileNameResult = New System.Text.StringBuilder()

        Dim rootPath As DirectoryInfo = New DirectoryInfo(Server.MapPath(FilePath))
        Dim allFiles As FileInfo() = rootPath.GetFiles("*", SearchOption.TopDirectoryOnly)

        If allFiles.Length > 0 Then
            Dim files As List(Of FileInfo) = New List(Of FileInfo)(allFiles)

            files.Sort(New FileInfoCompare())

            For Each file As FileInfo In files

                Dim fileRealName As String = GetFileRealFile(file.Name)
                FileNameResult.Append(String.Format(FileNameFormat, file.Name, fileRealName, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")))

            Next

            FileListStr = FileNameResult.ToString()
        Else
            FileListStr = "<tr><td style=""color:#888""><i>Nothing yet</i></td></tr>"
        End If

    End Sub

    Private Sub DeleteFile(ByVal fileName As String)
        Dim orgFileName As String = Server.MapPath(String.Concat(FilePath, fileName))

        Dim configFileName As String = Server.MapPath(String.Concat(FilePath, "xml/", fileName, ".config"))

        If File.Exists(orgFileName) Then
            Try
                File.Delete(orgFileName)
                File.Delete(configFileName)
            Catch
            End Try
        End If
    End Sub

    Private Sub DownloadFile(ByVal fileName As String)
        Dim realFileName As String = GetFileRealFile(fileName)
        Response.Clear()
        Response.AddHeader("Content-Type", "Application/Octect-Stream")
        Dim fileNameHeader As String = "attachment;filename={0}"

        Dim userAgent As String = Request.UserAgent.ToLower()
        Dim isIE As Boolean = (userAgent.IndexOf("msie") > -1)
        Dim isFirefox As Boolean = (userAgent.IndexOf("firefox") > -1)

        Dim fileNameContent As String = realFileName
        If (isIE) Then
            fileNameContent = Uri.EscapeUriString(realFileName)
        ElseIf (isFirefox) Then
            fileNameContent = String.Concat("""", realFileName, """")
        End If

        fileNameHeader = String.Format(fileNameHeader, fileNameContent)

        Response.AddHeader("Content-Disposition", fileNameHeader)

        Response.WriteFile(Server.MapPath(String.Concat(FilePath, fileName)))
        Response.End()
    End Sub

    Private Function GetFileRealFile(ByVal fileName As String) As String
        Dim fnIndex As String = fileName.LastIndexOf(Path.DirectorySeparatorChar)
        If fnIndex = -1 Then
            fnIndex = 0
        End If

        Dim guidName As String = fileName.Substring(fnIndex)

        Dim fileRealName As String = File.ReadAllText(Server.MapPath(String.Concat(FilePath, "/xml/", guidName, ".config")))

        Return fileRealName
    End Function

End Class


