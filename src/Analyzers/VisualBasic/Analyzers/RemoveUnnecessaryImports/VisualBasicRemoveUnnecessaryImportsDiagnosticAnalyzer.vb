﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.PooledObjects
Imports Microsoft.CodeAnalysis.RemoveUnnecessaryImports
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.RemoveUnnecessaryImports
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Friend NotInheritable Class VisualBasicRemoveUnnecessaryImportsDiagnosticAnalyzer
        Inherits AbstractRemoveUnnecessaryImportsDiagnosticAnalyzer

        Private Shared ReadOnly s_TitleAndMessageFormat As LocalizableString =
            New LocalizableResourceString(NameOf(VisualBasicAnalyzersResources.Imports_statement_is_unnecessary), VisualBasicAnalyzersResources.ResourceManager, GetType(VisualBasicAnalyzersResources))

        Protected Overrides Function GetTitleAndMessageFormatForClassificationIdDescriptor() As LocalizableString
            Return s_TitleAndMessageFormat
        End Function

        Protected Overrides ReadOnly Property UnnecessaryImportsProvider As IUnnecessaryImportsProvider
            Get
                Return VisualBasicUnnecessaryImportsProvider.Instance
            End Get
        End Property

        Protected Overrides Function IsRegularCommentOrDocComment(trivia As SyntaxTrivia) As Boolean
            Return trivia.IsRegularOrDocComment()
        End Function

        ''' Takes the import clauses we want to remove and returns them *or* their 
        ''' containing ImportsStatements *if* we wanted to remove all the clauses of
        ''' that ImportStatement.
        Protected Overrides Function MergeImports(unnecessaryImports As ImmutableArray(Of SyntaxNode)) As ImmutableArray(Of SyntaxNode)
            Dim result = ArrayBuilder(Of SyntaxNode).GetInstance()
            Dim importsClauses = unnecessaryImports.ToImmutableHashSet()

            For Each clause In importsClauses
                If Not result.Contains(clause.Parent) Then
                    Dim statement = DirectCast(clause.Parent, ImportsStatementSyntax)
                    If statement.ImportsClauses.All(AddressOf importsClauses.Contains) Then
                        result.Add(statement)
                    Else
                        result.Add(clause)
                    End If
                End If
            Next

            Return result.ToImmutableAndFree()
        End Function

        Protected Overrides Function GetFixableDiagnosticSpans(
                nodes As IEnumerable(Of SyntaxNode), tree As SyntaxTree, cancellationToken As CancellationToken) As IEnumerable(Of TextSpan)
            ' Create one fixable diagnostic that contains the entire Imports list.
            Return SpecializedCollections.SingletonEnumerable(tree.GetCompilationUnitRoot().Imports.GetContainedSpan())
        End Function

        Protected Overrides Function GetLastTokenDelegateForContiguousSpans() As Func(Of SyntaxNode, SyntaxToken)
            Return Function(n)
                       Dim lastToken = n.GetLastToken()
                       Return If(lastToken.GetNextToken().Kind = SyntaxKind.CommaToken,
                              lastToken.GetNextToken(),
                              lastToken)
                   End Function
        End Function
    End Class
End Namespace
