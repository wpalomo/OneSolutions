﻿Imports System.Data.SqlClient
Imports System.Xml
Imports System.Threading
Imports System.IO
Imports System.Windows.Forms
Public Class coneccion

   
    Public Function MakeConnection(Lista As ArrayList) As Boolean
        Dim Connected As Boolean = False
        Dim archivo As String = ""
        '' Dim cnnParam As New Settings
        Try

            Dim connectionString As String = "Server=" & Lista(0).ToString & ";Database=" & Lista(1).ToString & ";User Id=" & Lista(2).ToString & ";Password=" & Lista(3).ToString
            Using sqlCon = New SqlConnection(connectionString)
                sqlCon.Open()
                Dim sqlText = "select idsap,CodigoServicio,Total,Monto,CONVERT(VARCHAR(10), Fecha, 101),porcentaje,id_cgdesa from Datos where estado = 0 order by idsap, fecha asc"
                Dim cmd = New SqlCommand(sqlText, sqlCon)

                Dim reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    archivo = reader(1).ToString
                    Dim writer As New XmlTextWriter("Orden (O) No." & reader(6).ToString & ".xml", System.Text.Encoding.UTF8)
                    writer.WriteStartDocument(True)
                    writer.Formatting = Formatting.Indented
                    writer.Indentation = 2
                    'writer.WriteAttributeString("id", "comprobante")
                    'writer.WriteAttributeString("version", "2.0.0")
                    writer.WriteStartElement("order")
                    writer.WriteStartElement("document")
                    createNode("series", "", writer)
                    'createNode("docnum", reader(1).ToString, writer
                    'createNode("docdate", Date.Now.ToShortDateString, writer)
                    createNode("docdate", Date.Parse(reader(4).ToString).ToString("yyyMMdd"), writer)
                    'createNode("docdate", reader(4).ToString, writer)
                    createNode("doctotal", reader(3).ToString, writer)
                    createNode("cardcode", reader(0).ToString.Trim, writer)
                    createNode("doctype", "I", writer)
                    createNode("descuento", reader(5).ToString, writer)


                    'cmd = Nothing
                    sqlText = "SELECT * FROM Tipo_servicio where codigo ='" & reader(1).ToString.Trim & "' and tipo = 1 "
                    Dim sqlcon2 As New SqlConnection(connectionString)
                    sqlcon2.Open()
                    cmd = New SqlCommand(sqlText, sqlcon2)
                    Dim reader2 As SqlDataReader = cmd.ExecuteReader()
                    writer.WriteStartElement("document_lines")
                    'While reader2.Read()
                    If reader2.Read = True Then
                        writer.WriteStartElement("line")
                        createNode("itemcode", reader(1).ToString.Trim, writer)
                        createNode("quantity", "1", writer)
                        createNode("taxcode", "IVA", writer)
                        createNode("linetotal", "0.00", writer)
                        createNode("price", reader(3).ToString, writer)
                        createNode("descuento", reader(5).ToString, writer)
                        writer.WriteEndElement()

                    Else
                        writer.WriteStartElement("line")
                        createNode("itemcode", reader(1).ToString.Trim, writer)
                        createNode("quantity", reader(2).ToString, writer)
                        createNode("taxcode", "IVA", writer)
                        createNode("linetotal", "0.00", writer)
                        createNode("price", "0.00", writer)
                        createNode("descuento", reader(5).ToString, writer)
                        writer.WriteEndElement()
                    End If

                    ' End While
                    sqlcon2.Close()
                    'fin document line
                    writer.WriteEndElement()

                    'fin orden de Documento
                    writer.WriteEndElement()

                    writer.WriteEndElement()
                    writer.WriteEndDocument()
                    writer.Close()
                    'File.Move(System.IO.File.ReadAllText(Application.StartupPath & "\" & "Orden (O) No." & reader(1).ToString & ".xml"), System.IO.File.ReadAllText(Application.StartupPath & "\xml"))
                    Dim p = Application.StartupPath & "\" & "Orden (O) No." & reader(6).ToString & ".xml"
                    Dim o = Application.StartupPath & "\xml\ Orden (O) No." & reader(6).ToString & ".xml"
                    If File.Exists(o) Then
                        File.Delete(o)
                    End If
                    File.Move(p, o)
                    sqlText = "UPDATE DATOS SET estado = 1 WHERE  id_cgdesa =" & reader(6).ToString
                    sqlcon2 = New SqlConnection(connectionString)
                    sqlcon2.Open()
                    cmd = New SqlCommand(sqlText, sqlcon2)
                    Dim a = cmd.ExecuteNonQuery()
                    sqlcon2.Close()
                End While
                sqlCon.Close()
            End Using
            Return Connected
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Function

    Private Sub createNode(ByVal pID As String, ByVal pName As String, ByVal writer As XmlTextWriter)
        writer.WriteStartElement(pID)
        writer.WriteString(pName)
        writer.WriteEndElement()
    End Sub

    Public Sub ordenVenta()



    End Sub
    Public Function cargarConfiguaracion() As ArrayList

        Dim Lista As ArrayList = New ArrayList()
        Try
            Dim Doc As New XmlDocument, ListaNodos As XmlNodeList, Nodo As XmlNode
            Dim encrip As New EncryptComp.Library.Encrypt
            Dim mytext = System.IO.File.ReadAllText(Application.StartupPath & "\CONFIGURACION.xml")
            mytext = encrip.DecryptKey(mytext)
            Dim xmldoc = XDocument.Parse(mytext)
            xmldoc.Save(Application.StartupPath & "\CONFIGURACION.xml")

            Doc.Load(Application.StartupPath & "\CONFIGURACION.xml")

            ListaNodos = Doc.SelectNodes("/CONFIGURACION/PARAMETRO")

            For Each Nodo In ListaNodos
                Lista.Add(Nodo.ChildNodes.Item(0).InnerText)
            Next
            Dim mytext2 = System.IO.File.ReadAllText(Application.StartupPath & "\CONFIGURACION.xml")
            mytext2 = encrip.EncryptKey(mytext)
            System.IO.File.WriteAllText(Application.StartupPath & "\CONFIGURACION.xml", mytext2)
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
            Lista = New ArrayList
        End Try
        Return Lista
    End Function

   

End Class
