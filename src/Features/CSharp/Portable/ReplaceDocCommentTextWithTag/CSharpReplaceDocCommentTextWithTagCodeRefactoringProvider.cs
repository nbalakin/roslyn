﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.ReplaceDocCommentTextWithTag;

namespace Microsoft.CodeAnalysis.CSharp.ReplaceDocCommentTextWithTag
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp), Shared]
    internal class CSharpReplaceDocCommentTextWithTagCodeRefactoringProvider :
        AbstractReplaceDocCommentTextWithTagCodeRefactoringProvider
    {
        private static HashSet<string> s_keywords = new HashSet<string>
        {
            SyntaxFacts.GetText(SyntaxKind.NullKeyword),
            SyntaxFacts.GetText(SyntaxKind.StaticKeyword),
            SyntaxFacts.GetText(SyntaxKind.VirtualKeyword),
            SyntaxFacts.GetText(SyntaxKind.TrueKeyword),
            SyntaxFacts.GetText(SyntaxKind.FalseKeyword),
            SyntaxFacts.GetText(SyntaxKind.AbstractKeyword),
            SyntaxFacts.GetText(SyntaxKind.SealedKeyword),
            SyntaxFacts.GetText(SyntaxKind.AsyncKeyword),
            SyntaxFacts.GetText(SyntaxKind.AwaitKeyword)
        };

        protected override bool IsXmlTextToken(SyntaxToken token)
            => token.Kind() == SyntaxKind.XmlTextLiteralToken ||
               token.Kind() == SyntaxKind.XmlTextLiteralNewLineToken;

        protected override bool IsInXMLAttribute(SyntaxToken token)
        {
            return (token.Parent.Kind() == SyntaxKind.XmlCrefAttribute
                || token.Parent.Kind() == SyntaxKind.XmlNameAttribute
                || token.Parent.Kind() == SyntaxKind.XmlTextAttribute);
        }

        protected override bool IsKeyword(string text)
        {
            return s_keywords.Contains(text);
        }

        protected override SyntaxNode ParseExpression(string text)
            => SyntaxFactory.ParseExpression(text);

    }
}
