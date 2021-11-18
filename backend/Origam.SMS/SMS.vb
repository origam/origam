'==========================================================
'                    SMS,EMS Decoder
'                      2004-9-24
'1.Description
'   This class decode a SMS or EMS PDU code to a certain
'class. You can use it in your software to read SMSs and 
'EMSs. All of this is done under GSM 03.40. I tested it
'on my SIEMENS M55 and NOKIA 8xxx and it works well.
'2.Useage 
'   If you know what type of PDU code, you can create a 
'new instance of class like DIM s as SMS(myPDUCode)
'When instance is created, you read its public variable 
'to get what you want.
'   When TP_DCS=0, PDU code is coded from 7bit 
'charactor (see GSM 03.38), use shared function 
'Deocde7Bit to decode it.
'   When TP_DCS=8, PDU code is coded from Unicode
'charactor (see GSM 03.38), use shared funtion
'DecodeUnicode to decode it.
'3.Bugs
'   So far in my tests I found none.
'4.When you use it
'   You can freely use it or modify it in your program,
'but when you find bugs or improved it please publish it
'or send one copy to me. Thanks
'5.About me
'   I am writting a program which can list folders and 
'files in SIEMENS M55 mobile phone. It can also read
'and send SMS,EMS. Some documents are hard to find on
'internet, but I keep on my mind to study it and finally
'I found it is full of interests.
'    I like freedom, so'I exchange my ideas with all of 
'the world. It is so happy that you can use my classes!
'   In the end, sorry for my poor english.
'6.Contact me
'   Email:hesicong@mail.sc.cninfo.net
'   QQ:38288890
'   Homepage:http://dream-world.nease.net (Chinese)
'   Thanks for using it!
'                       ----By HESICONG
'
'Revision:
'   2004-10-29:
'       Fix bug in decode "@" charactor.
'       Add functions in "decode 7 bit code to english" region


Imports System.text
Namespace Decoder
    Public MustInherit Class SMSBase
        'Note all of following various with TP_ can be found in GSM 03.40
        Public SCAddressLength As Byte  'Service Center Address length
        Public SCAddressType As Byte    'Service Center Type[See GSM 03.40]
        Public SCAddressValue As String 'Service Center nuber
        Public FirstOctet As Byte       'See GSM 03.40

        Public TP_PID As Byte
        Public TP_DCS As Byte
        Public TP_UDL As Byte
        Public TP_UD As String
        Public Text As String
        Public Type As SMSType
        Public UserData As String

        Public Enum SMSType
            SMS_RECEIVED = 0
            SMS_STATUS_REPORT = 2
            SMS_SUBMIT = 1
            EMS_RECEIVED = 64 'It is "Reserved" on my phone??
            EMS_SUBMIT = 65
        End Enum

        Public MustOverride Sub GetOrignalData(ByVal PDUCode As String)

        'Get a byte from PDU string
        Shared Function GetByte(ByRef PDUCode As String) As Byte
            Dim r As Byte = Val("&H" + Mid(PDUCode, 1, 2))
            PDUCode = Mid(PDUCode, 3)
            Return r
        End Function

        'Get a string of certain length
        Shared Function GetString(ByRef PDUCode As String, ByVal Length As Integer) As String
            Dim r As String = Mid(PDUCode, 1, Length)
            PDUCode = Mid(PDUCode, Length + 1)
            Return r
        End Function

        'Get date from SCTS format
        Shared Function GetDate(ByRef SCTS As String) As Date
            Dim year, month, day, hour, minute, second, timezone As Integer

            year = Val(Swap(GetString(SCTS, 2))) + 2000
            month = Val(Swap(GetString(SCTS, 2)))
            day = Val(Swap(GetString(SCTS, 2)))
            hour = Val(Swap(GetString(SCTS, 2)))
            minute = Val(Swap(GetString(SCTS, 2)))
            second = Val(Swap(GetString(SCTS, 2)))
            timezone = Val(Swap(GetString(SCTS, 2)))

            Dim result As New Date(year, month, day, hour, minute, second)
            Return result
        End Function

        'Swap two bit
        Shared Function Swap(ByRef TwoBitStr As String) As String
            Dim c() As Char = TwoBitStr.ToCharArray
            Dim t As Char
            t = c(0)
            c(0) = c(1)
            c(1) = t
            Return (c(0) + c(1)).ToString
        End Function

        'Get phone address
        Shared Function GetAddress(ByRef Address As String) As String
            Dim tmpChar As Char() = Address.ToCharArray
            Dim i As Integer, result As String
            For i = 0 To tmpChar.GetUpperBound(0) Step 2
                result += Swap(tmpChar(i) + tmpChar(i + 1))
            Next
            If InStr(result, "F") Then result = Mid(result, 1, result.Length - 1)
            Return result
        End Function

        Shared Function GetSMSType(ByVal PDUCode As String) As SMSBase.SMSType
            'Get first october
            Dim FirstOctet As Byte
            Dim L As Integer = SMSBase.GetByte(PDUCode)
            SMSBase.GetByte(PDUCode)
            SMSBase.GetString(PDUCode, (L - 1) * 2)
            FirstOctet = SMSBase.GetByte(PDUCode)
            '[Chinese]取得特征码
            '[Chinese]取得基本码 最后两个bit + 是否有header作为标记
            'Get base code. Use last 2 bit and whether there's a header as remark
            Dim t1 As Integer = FirstOctet And 3 '00000011
            Dim t2 As Integer = FirstOctet And 64 '01000000
            '[Chinese]特别处理
            If t1 = 3 And t2 = 64 Then Return SMSBase.SMSType.EMS_SUBMIT
            Return t1 + t2
        End Function

        'Deoce a unicode string
        Shared Function DecodeUnicode(ByVal strUnicode As String) As String
            Dim Code As String
            Dim i, j As Integer
            Dim c() As String       'temp
            ReDim c(strUnicode.Length / 4)     '2 Byte a Unicode char

            For j = 0 To strUnicode.Length \ 4 - 1
                Dim d() As Char = strUnicode.ToCharArray(j * 4, 4)
                c(j) = "&H" & CType(d, String)
                c(j) = ChrW(Val(c(j)))
                Code += c(j)
            Next
            Return Code
        End Function

#Region "'Deocde 7Bit to english"
        '2004-10-29:Region added
        'Fixed decode "@" charactor
        'I use a new method, it is easily to understand but look much longer than before.
        Shared Function InvertHexString(ByVal HexString As String) As String
            'For example:
            '123456
            '===>
            '563412
            Dim Result As New StringBuilder
            Dim i As Integer
            For i = HexString.Length - 2 To 0 Step -2
                Result.Append(HexString.Substring(i, 2))
            Next
            Return Result.ToString
        End Function

        Shared Function ByteToBinary(ByVal Dec As Byte) As String
            Dim Result As String
            Dim Temp As Byte = Dec
            Do
                Result = (Temp Mod 2) & Result
                If Temp = 1 Or Temp = 0 Then Exit Do
                Temp = Temp \ 2
            Loop
            Result = Result.PadLeft(8, "0")
            Return Result
        End Function

        Shared Function BinaryToInt(ByVal Binary As String) As Integer
            Dim Result As Integer
            Dim i As Integer
            For i = 0 To Binary.Length - 1
                Result = Result + Val(Binary.Substring(Binary.Length - i - 1, 1)) * 2 ^ i
            Next
            Return Result
        End Function

        Shared Function Decode7Bit(ByVal str7BitCode As String, ByVal charCount As Integer) As String
            Dim Inv7BitCode As String = InvertHexString(str7BitCode)
            Dim Binary As String
            Dim Result As String
            Dim i As Integer
            For i = 0 To Inv7BitCode.Length - 1 Step 2
                Binary += ByteToBinary(Val("&H" & Inv7BitCode.Substring(i, 2)))
            Next
            Dim Temp As Integer
            For i = 1 To charCount
                Temp = BinaryToInt(Binary.Substring(Binary.Length - i * 7, 7))
                'There is a problem:
                '"@" charactor is decoded to binary "0000000", but its Ascii Code is 64!!
                'Don't know what to do with it,maybe it is a bug!
                If Temp = 0 Then Temp = 64
                Result = Result + ChrW(Temp)
            Next
            Return (Result)
        End Function
#End Region
    End Class

    Public Class SMS_RECEIVED
        Inherits SMSBase
        Public SrcAddressLength As Byte
        Public SrcAddressType As Byte
        Public SrcAddressValue As String
        Public TP_SCTS As Date

        Sub New(ByVal PDUCode As String)
            Type = SMSBase.SMSType.SMS_RECEIVED
            GetOrignalData(PDUCode)
        End Sub
        Public Overrides Sub GetOrignalData(ByVal PDUCode As String)
            SCAddressLength = GetByte(PDUCode)
            SCAddressType = GetByte(PDUCode)
            SCAddressValue = GetAddress((GetString(PDUCode, (SCAddressLength - 1) * 2)))
            FirstOctet = GetByte(PDUCode)

            SrcAddressLength = GetByte(PDUCode)
            SrcAddressType = GetByte(PDUCode)
            SrcAddressLength += SrcAddressLength Mod 2
            SrcAddressValue = GetAddress((GetString(PDUCode, SrcAddressLength)))


            TP_PID = GetByte(PDUCode)
            TP_DCS = GetByte(PDUCode)
            TP_SCTS = GetDate(GetString(PDUCode, 14))
            TP_UDL = GetByte(PDUCode)
            TP_UD = GetString(PDUCode, TP_UDL * 2)
        End Sub
    End Class

    Public Class SMS_SUBMIT
        Inherits SMSBase
        Public TP_MR As Byte
        Public DesAddressLength As Byte
        Public DesAddressType As Byte
        Public DesAddressValue As String
        Public TP_VP As Byte
        Sub New(ByVal PDUCode As String)
            Type = SMSBase.SMSType.SMS_SUBMIT
            GetOrignalData(PDUCode)
        End Sub

        Public Overrides Sub GetOrignalData(ByVal PDUCode As String)
            SCAddressLength = GetByte(PDUCode)
            SCAddressType = GetByte(PDUCode)
            SCAddressValue = GetAddress((GetString(PDUCode, (SCAddressLength - 1) * 2)))
            FirstOctet = GetByte(PDUCode)

            TP_MR = GetByte(PDUCode)

            DesAddressLength = GetByte(PDUCode)
            DesAddressType = GetByte(PDUCode)
            DesAddressLength += DesAddressLength Mod 2
            DesAddressValue = GetAddress((GetString(PDUCode, DesAddressLength)))

            TP_PID = GetByte(PDUCode)
            TP_DCS = GetByte(PDUCode)
            TP_VP = GetByte(PDUCode)
            TP_UDL = GetByte(PDUCode)
            TP_UD = GetString(PDUCode, TP_UDL * 2)
        End Sub
    End Class

    Public Class EMS_RECEIVED
        Inherits SMS_RECEIVED
        Public Structure InfoElem       'See document "How to create EMS"
            Public Identifier As Byte
            Public Length As Byte
            Public Data As String
        End Structure
        Public TP_UDHL As Byte

        Public IE() As InfoElem

        Sub New(ByVal PDUCode As String)
            MyBase.New(PDUCode)
        End Sub
        Public Overrides Sub GetOrignalData(ByVal PDUCode As String)
            SCAddressLength = GetByte(PDUCode)
            SCAddressType = GetByte(PDUCode)
            SCAddressValue = GetAddress(GetString(PDUCode, (SCAddressLength - 1) * 2))
            FirstOctet = GetByte(PDUCode)

            SrcAddressLength = GetByte(PDUCode)
            SrcAddressType = GetByte(PDUCode)
            SrcAddressLength += SrcAddressLength Mod 2
            SrcAddressValue = GetAddress((GetString(PDUCode, SrcAddressLength)))

            TP_PID = GetByte(PDUCode)
            TP_DCS = GetByte(PDUCode)
            TP_SCTS = GetDate(GetString(PDUCode, 14))
            TP_UDL = GetByte(PDUCode)
            TP_UDHL = GetByte(PDUCode)

            IE = GetIE(GetString(PDUCode, TP_UDHL * 2))

            TP_UD = GetString(PDUCode, TP_UDL * 2)
        End Sub

        'Get Informat Elements 
        Shared Function GetIE(ByVal IECode As String) As InfoElem()
            Dim tmp As String = IECode, t As Integer = 0
            Dim result() As InfoElem
            Do Until IECode = ""
                ReDim Preserve result(t)
                With result(t)
                    .Identifier = GetByte(IECode)
                    .Length = GetByte(IECode)
                    .Data = GetString(IECode, .Length * 2)
                End With
                t += 1
            Loop
            Return result
        End Function
    End Class

    Public Class EMS_SUBMIT
        Inherits SMS_SUBMIT

        Sub New(ByVal PDUCode As String)
            MyBase.New(PDUCode)
            Type = SMSBase.SMSType.EMS_SUBMIT
        End Sub

        Public TP_UDHL As Byte

        Public IE() As EMS_RECEIVED.InfoElem


        Public Overrides Sub GetOrignalData(ByVal PDUCode As String)
            SCAddressLength = GetByte(PDUCode)
            SCAddressType = GetByte(PDUCode)
            SCAddressValue = GetAddress(GetString(PDUCode, (SCAddressLength - 1) * 2))
            FirstOctet = GetByte(PDUCode)

            TP_MR = GetByte(PDUCode)

            DesAddressLength = GetByte(PDUCode)
            DesAddressType = GetByte(PDUCode)
            DesAddressLength += DesAddressLength Mod 2
            DesAddressValue = GetAddress(GetString(PDUCode, DesAddressLength))

            TP_PID = GetByte(PDUCode)
            TP_DCS = GetByte(PDUCode)
            TP_VP = GetByte(PDUCode)
            TP_UDL = GetByte(PDUCode)

            TP_UDHL = GetByte(PDUCode)
            IE = EMS_RECEIVED.GetIE(GetString(PDUCode, TP_UDHL * 2))

            TP_UD = GetString(PDUCode, TP_UDL * 2)
        End Sub
    End Class

    Public Class SMS_STATUS_REPORT
        Inherits SMS_RECEIVED
        Public TP_MR As Byte
        Public TP_DP As Date
        Public Status As EnumStatus

        Public Enum EnumStatus
            Success = 0
            NotSend = 96
            NoResponseFromSME = 98
        End Enum

        Sub New(ByVal PDUCode As String)
            MyBase.New(PDUCode)
            Type = SMSBase.SMSType.SMS_STATUS_REPORT
        End Sub
        Public Overrides Sub GetOrignalData(ByVal PDUCode As String)
            SCAddressLength = GetByte(PDUCode)
            SCAddressType = GetByte(PDUCode)
            SCAddressValue = GetAddress(GetString(PDUCode, (SCAddressLength - 1) * 2))

            FirstOctet = GetByte(PDUCode)

            TP_MR = GetByte(PDUCode)

            SrcAddressLength = GetByte(PDUCode)
            SrcAddressType = GetByte(PDUCode)
            SrcAddressLength += SrcAddressLength Mod 2
            SrcAddressValue = GetAddress(GetString(PDUCode, SrcAddressLength))

            TP_SCTS = GetDate(GetString(PDUCode, 14))
            TP_DP = GetDate(GetString(PDUCode, 14))

            Status = GetByte(PDUCode)

            'Status report do not have content so I set it a zero length string
            TP_UD = ""
        End Sub
    End Class

    Public Class PDUDecoder
        Public Structure BaseInfo
            Public SourceNumber As String
            Public DestinationNumber As String
            Public ReceivedDate As Date
            Public Text As String
            Public Type As SMS.Decoder.SMSBase.SMSType
            Public EMSTotolPiece As Integer
            Public EMSCurrentPiece As Integer
            Public StatusFromReport As SMS_STATUS_REPORT.EnumStatus

            Public DestinationReceivedDate
        End Structure

        Sub New(ByVal PDUCode As String)
            Decode(PDUCode)
        End Sub

        Public Shared Function Decode(ByVal PDUCode As String) As BaseInfo
            Dim Result As BaseInfo
            Try
                Dim s As Object
                Dim T As SMSBase.SMSType = SMSBase.GetSMSType(PDUCode)
                Result.Type = T
                Select Case T
                    Case SMSBase.SMSType.EMS_RECEIVED
                        s = New EMS_RECEIVED(PDUCode)
                        Result.SourceNumber = s.SrcAddressValue

                        If s.SrcAddressType = &H91 Then Result.SourceNumber = "+" + Result.SourceNumber

                        Result.ReceivedDate = s.TP_SCTS

                        If Not (s.IE Is Nothing) Then
                            Dim Data As String = s.IE(0).Data
                            Result.EMSTotolPiece = CInt(Mid(Data, 5, 2))
                            Result.EMSCurrentPiece = CInt(Mid(Data, 7, 2))
                        End If

                    Case SMSBase.SMSType.SMS_RECEIVED
                        s = New SMS_RECEIVED(PDUCode)
                        Result.SourceNumber = s.SrcAddressValue
                        If s.SrcAddressType = &H91 Then Result.SourceNumber = "+" + Result.SourceNumber
                        Result.ReceivedDate = s.TP_SCTS

                    Case SMSBase.SMSType.EMS_SUBMIT
                        s = New EMS_SUBMIT(PDUCode)

                        Result.DestinationNumber = s.DesAddressValue
                        If s.DesAddressType = &H91 Then Result.DestinationNumber = "+" + Result.DestinationNumber
                        If Not (s.IE Is Nothing) Then
                            Dim Data As String = s.IE(0).Data
                            Result.EMSTotolPiece = CInt(Mid(Data, 5, 2))
                            Result.EMSCurrentPiece = CInt(Mid(Data, 7, 2))
                        End If

                    Case SMSBase.SMSType.SMS_SUBMIT
                        s = New SMS_SUBMIT(PDUCode)
                        Result.DestinationNumber = s.DesAddressValue
                        If s.DesAddressType = &H91 Then Result.DestinationNumber = "+" + Result.DestinationNumber

                    Case SMSBase.SMSType.SMS_STATUS_REPORT
                        s = New SMS_STATUS_REPORT(PDUCode)
                        Result.SourceNumber = s.SrcAddressValue
                        If s.SrcAddressType = &H91 Then Result.SourceNumber = "+" + Result.SourceNumber
                        Result.ReceivedDate = s.TP_SCTS
                        Result.DestinationReceivedDate = s.TP_DP()
                        Result.StatusFromReport = s.status
                    Case Else
                        Stop
                End Select
                '###########################
                'Correct when s is SMS type, no TP_UDL is found.
                'Note:Only EMS has the TP_UDHL and TP_UDH see 3GPP TS 23.040 V6.5.0 (2004-09)
                '###########################
                If s.tp_DCS = 0 Then
                    If T = SMSBase.SMSType.SMS_RECEIVED Or T = SMSBase.SMSType.SMS_STATUS_REPORT Or T = SMSBase.SMSType.SMS_SUBMIT Then
                        '#############################
                        'add a parameter
                        '############################
                        Result.Text = s.decode7bit(s.tp_UD, s.TP_UDL)
                    End If

                    If T = SMSBase.SMSType.EMS_RECEIVED Or T = SMSBase.SMSType.EMS_SUBMIT Then
                        Result.Text = s.decode7bit(s.tp_ud, s.tp_udl - 8 * (1 + s.tp_udhl) / 7)
                    End If
                Else
                    Result.Text = s.DecodeUnicode(s.TP_UD)
                End If
            Catch err As Exception
                Result.Text = "Error:" & PDUCode
            End Try
            Return Result
        End Function
    End Class

End Namespace
