Sub BoldBracketedText()
    Dim cell As Range
    Dim txt As String
    Dim i As Long, startPos As Long
    Dim inBracket As Boolean

    For Each cell In Selection
        txt = cell.Value
        If Len(txt) = 0 Then GoTo NextCell

        inBracket = False
        For i = 1 To Len(txt)
            If Mid(txt, i, 1) = "[" Then
                inBracket = True
                startPos = i + 1
            ElseIf Mid(txt, i, 1) = "]" And inBracket Then
                If i > startPos Then
                    cell.Characters(startPos, i - startPos).Font.Bold = True
                End If
                inBracket = False
            End If
        Next i
NextCell:
    Next cell
End Sub
