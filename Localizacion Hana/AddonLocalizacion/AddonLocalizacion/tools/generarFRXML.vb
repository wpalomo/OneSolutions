﻿Imports System.Xml
Imports System.IO

Public Class generarFRXML
    Public Sub generarXML(DocEntry As String, objectType As String, oCompany As SAPbobsCOM.Company, SBO As SAPbouiCOM.Application)
        Try
            Dim doc As New XmlDocument
            Dim oRecord As SAPbobsCOM.Recordset
            oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRecord.DoQuery("SELECT A." & Chr(34) & "DocEntry" & Chr(34) & " From OPCH A Where A." & Chr(34) & "DocEntry" & Chr(34) & "= " & DocEntry & " And A.U_TI_COMPRO ='41'")
            If oRecord.RecordCount > 0 Then
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL ENCABEZADO_FACTURA ('" & DocEntry & "','FR')")
                Dim writer As New XmlTextWriter("Comprobante (FR) No." & DocEntry.ToString & ".xml", System.Text.Encoding.UTF8)
                writer.WriteStartDocument(True)
                writer.Formatting = Formatting.Indented
                writer.Indentation = 2
                writer.WriteStartElement("factura")
                writer.WriteAttributeString("id", "comprobante")
                writer.WriteAttributeString("version", "2.0.0")
                writer.WriteStartElement("infoTributaria")
                createNode("razonSocial", oRecord.Fields.Item(2).Value.ToString, writer)
                'createNode("ambiente", oRecord.Fields.Item(0).Value.ToString, writer)
                'createNode("tipoEmision", oRecord.Fields.Item(1).Value.ToString, writer)
                createNode("ruc", oRecord.Fields.Item(3).Value.ToString.PadLeft(13, "0"), writer)
                'createNode("claveAcesso", claveAcceso(oRecord).PadLeft(49, "0"), writer)
                'createNode("claveAcesso", "", writer)
                createNode("codDoc", oRecord.Fields.Item("codDoc").Value.ToString.PadLeft(2, "0"), writer)
                createNode("estab", oRecord.Fields.Item("estab").Value.ToString.PadLeft(3, "0"), writer)
                createNode("ptoEmi", oRecord.Fields.Item("ptoEmi").Value.ToString.PadLeft(3, "0"), writer)
                createNode("secuencial", oRecord.Fields.Item("secuencial").Value.ToString.PadLeft(9, "0"), writer)
                createNode("dirMatriz", oRecord.Fields.Item("dirMatriz").Value.ToString, writer)
                Dim direccion = oRecord.Fields.Item("dirMatriz").Value.ToString
                Dim contribuyenteEspecial = oRecord.Fields.Item("contriespecial").Value.ToString
                Dim obliConta = oRecord.Fields.Item("contaobligado").Value.ToString
                ''Cierre info Tributaria
                writer.WriteEndElement()

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                writer.WriteStartElement("infoFactura")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL SP_INFO_FACTURA ('" & DocEntry & "','FR')")
                createNode("fechaEmision", Date.Parse(oRecord.Fields.Item("DATE").Value.ToString).ToString("dd/MM/yyyy"), writer)
                createNode("dirEstablecimiento", oRecord.Fields.Item("DIRECCION").Value.ToString, writer)
                If contribuyenteEspecial <> "" Then
                    createNode("contribuyenteEspecial", contribuyenteEspecial, writer)
                End If
                createNode("obligadoContabilidad", oRecord.Fields.Item(3).Value, writer)
                createNode("tipoIdentificacionComprador", oRecord.Fields.Item("U_IDENTIFICACION").Value.ToString, writer)
                createNode("razonSocialComprador", oRecord.Fields.Item("CardName").Value.ToString, writer)
                createNode("identificacionComprador", oRecord.Fields.Item("U_DOCUMENTO").Value.ToString, writer)
                createNode("totalSinImpuestos", Double.Parse(oRecord.Fields.Item("sin_impuesto").Value).ToString("N2"), writer)
                createNode("totalDescuento", Double.Parse(oRecord.Fields.Item("totDescuento").Value.ToString).ToString("N2"), writer)
                createNode("codDocReembolso", "41", writer)
                Dim importeTotal = oRecord.Fields.Item("DocTotal").Value.ToString
                Dim moneda = oRecord.Fields.Item("MONEDA").Value.ToString
                createNode("totalComprobantesReembolso", Double.Parse(importeTotal).ToString("N2"), writer)
                createNode("totalBaseImponibleReembolso", Double.Parse(oRecord.Fields.Item("sin_impuesto").Value).ToString("N2"), writer)
                createNode("totalImpuestoReembolso", Double.Parse(oRecord.Fields.Item("VatSum").Value).ToString("N2"), writer)
                writer.WriteStartElement("totalConImpuestos")
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL SP_Total_Con_Impuesto ('" & DocEntry & "','FR')")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        writer.WriteStartElement("totalImpuesto")
                        createNode("codigo", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("codigoPorcentaje", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("baseImponible", Double.Parse(oRecord.Fields.Item(2).Value).ToString("N2"), writer)
                        createNode("tarifa", Double.Parse(oRecord.Fields.Item(3).Value).ToString("N2"), writer)
                        createNode("valor", Double.Parse(oRecord.Fields.Item(4).Value).ToString("N2"), writer)
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                ''Cierre TotalConImpuestos
                writer.WriteEndElement()
                createNode("propina", "0.00", writer)
                createNode("importeTotal", importeTotal, writer)
                createNode("moneda", moneda, writer)
                writer.WriteStartElement("pagos")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL SP_Forma_Pago ('" & DocEntry & "','FR')")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        writer.WriteStartElement("pago")
                        createNode("formaPago", oRecord.Fields.Item(0).Value, writer)
                        createNode("total", Double.Parse(oRecord.Fields.Item(1).Value).ToString("N2"), writer)
                        createNode("plazo", oRecord.Fields.Item(2).Value, writer)
                        createNode("unidadTiempo", oRecord.Fields.Item(3).Value, writer)
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If
                ''Cierre Pagos
                writer.WriteEndElement()



                ''Cierre INFO FACTURA
                writer.WriteEndElement()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                writer.WriteStartElement("detalles")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL SP_DetalleFac ('" & DocEntry & "','FR')")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        Dim oRecord2 As SAPbobsCOM.Recordset
                        oRecord2 = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        writer.WriteStartElement("detalle")
                        createNode("codigoPrincipal", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("descripcion", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("cantidad", oRecord.Fields.Item(2).Value.ToString, writer)
                        createNode("precioUnitario", Double.Parse(oRecord.Fields.Item(3).Value.ToString).ToString("N2"), writer)
                        createNode("descuento", Double.Parse(oRecord.Fields.Item(4).Value.ToString).ToString("N2"), writer)
                        createNode("precioTotalSinImpuesto", Double.Parse(oRecord.Fields.Item(6).Value).ToString("N2"), writer)
                        writer.WriteStartElement("impuestos")
                        oRecord2.DoQuery("CALL SP_Impuesto_Detalle ('" & DocEntry & "','" & oRecord.Fields.Item(0).Value.ToString & "','FR')")
                        If oRecord2.RecordCount > 0 Then
                            While oRecord2.EoF = False
                                writer.WriteStartElement("impuesto")
                                createNode("codigo", oRecord2.Fields.Item(0).Value.ToString, writer)
                                createNode("codigoPorcentaje", oRecord2.Fields.Item(1).Value.ToString, writer)
                                createNode("tarifa", Double.Parse(oRecord2.Fields.Item(3).Value).ToString("N2"), writer)
                                createNode("baseImponible", Double.Parse(oRecord2.Fields.Item(2).Value).ToString("N2"), writer)
                                createNode("valor", Double.Parse(oRecord2.Fields.Item(4).Value).ToString("N2"), writer)
                                writer.WriteEndElement()
                                oRecord2.MoveNext()
                            End While
                        End If

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord2)
                        oRecord2 = Nothing
                        GC.Collect()
                        writer.WriteEndElement()

                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If

                ''Cierre detalles
                writer.WriteEndElement()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()


                writer.WriteStartElement("reembolsos")

                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("CALL SP_DetalleFacReembolso ('" & DocEntry & "','13')")
                If oRecord.RecordCount > 0 Then
                    While (oRecord.EoF = False)
                        writer.WriteStartElement("reembolsoDetalle")
                        Dim oRecord2 As SAPbobsCOM.Recordset
                        oRecord2 = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        createNode("tipoIdentificacionProveedorReembolso", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("identificacionProveedorReembolso", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("codPaisPagoProveedorReembolso", oRecord.Fields.Item(2).Value.ToString, writer)
                        createNode("tipoProveedorReembolso", oRecord.Fields.Item(3).Value.ToString, writer)
                        createNode("codDocReembolso", oRecord.Fields.Item(4).Value.ToString, writer)
                        createNode("estabDocReembolso", oRecord.Fields.Item(5).Value.ToString, writer)
                        createNode("ptoEmiDocReembolso", oRecord.Fields.Item(6).Value.ToString, writer)
                        createNode("secuencialDocReembolso", oRecord.Fields.Item(7).Value.ToString.PadLeft(9, "0"), writer)
                        createNode("fechaEmisionDocReembolso", oRecord.Fields.Item(8).Value.ToString, writer)
                        createNode("numeroautorizacionDocReemb", oRecord.Fields.Item(9).Value.ToString, writer)
                        writer.WriteStartElement("detalleImpuestos")
                        oRecord2.DoQuery("CALL SP_Impuesto_Detalle ('" & DocEntry & "','" & oRecord.Fields.Item(0).Value.ToString & "','FR')")
                        If oRecord2.RecordCount > 0 Then
                            While oRecord2.EoF = False
                                writer.WriteStartElement("detalleImpuesto")
                                createNode("codigo", oRecord2.Fields.Item(0).Value.ToString, writer)
                                createNode("codigoPorcentaje", oRecord2.Fields.Item(1).Value.ToString, writer)
                                createNode("tarifa", Double.Parse(oRecord2.Fields.Item(3).Value).ToString("N2"), writer)
                                createNode("baseImponibleReembolso", Double.Parse(oRecord2.Fields.Item(2).Value).ToString("N2"), writer)
                                createNode("impuestoReembolso", Double.Parse(oRecord2.Fields.Item(4).Value).ToString("N2"), writer)
                                writer.WriteEndElement()
                                oRecord2.MoveNext()
                            End While
                        End If

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord2)
                        oRecord2 = Nothing
                        GC.Collect()
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While

                    ''Cierre Reembolso
                    writer.WriteEndElement()
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                ''Cierre Reembolso
                writer.WriteEndElement()



                ''Abre Campos Adicionales

                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                Dim en = "CALL SP_INFOADICIONAL ('" & DocEntry & "','FR')"
                oRecord.DoQuery(en)
                If oRecord.RecordCount > 0 Then
                    writer.WriteStartElement("infoAdicional")

                    While oRecord.EoF = False
                        writer.WriteStartElement("campoAdicional")
                        writer.WriteAttributeString("nombre", oRecord.Fields.Item("nombre").Value)
                        writer.WriteString(oRecord.Fields.Item("Valor").Value)
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                    writer.WriteEndElement()
                    'Cierre Campos Adicionales

                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                ''Cierre Factura
                writer.WriteEndElement()
                writer.WriteEndDocument()
                writer.Close()
                If Directory.Exists("C:\OS_FE") = False Then
                    Directory.CreateDirectory("C:\OS_FE")
                End If
                Dim esta = Application.StartupPath & "\Comprobante (FR) No." & DocEntry.ToString & ".xml"
                Dim va = "C:\OS_FE\Comprobante (FR) No." & DocEntry.ToString & ".xml"
                If File.Exists(va) Then
                    File.Delete(va)
                    File.Move(esta, va)
                Else
                    File.Move(esta, va)
                End If

                If My.Computer.FileSystem.FileExists(Application.StartupPath & "\CONFIGURACION.xml") = True Then
                    Dim Docc As New XmlDocument, ListaNodos As XmlNodeList, Nodo As XmlNode
                    Dim Lista As ArrayList = New ArrayList()
                    Docc.Load(Application.StartupPath & "\CONFIGURACION.xml")

                    ListaNodos = Docc.SelectNodes("/CONFIGURACION/PARAMETRO")

                    For Each Nodo In ListaNodos
                        Lista.Add(Nodo.ChildNodes.Item(0).InnerText)
                    Next
                    My.Computer.Network.UploadFile(va, Lista(0).ToString & "Comprobante (FR) No." & DocEntry.ToString & ".xml", Lista(1).ToString, Lista(2).ToString, True, 2500, FileIO.UICancelOption.DoNothing)
                End If
                
            Else
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub createNode(ByVal pID As String, ByVal pName As String, ByVal writer As XmlTextWriter)
        writer.WriteStartElement(pID)
        writer.WriteString(pName)
        writer.WriteEndElement()
    End Sub
End Class
