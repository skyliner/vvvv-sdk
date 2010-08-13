﻿// CSharp Editor Example with Code Completion
// Copyright (c) 2006, Daniel Grunwald
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other materials
//   provided with the distribution.
// 
// - Neither the name of the ICSharpCode team nor the names of its contributors may be used to
//   endorse or promote products derived from this software without specific prior written
//   permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using VVVV.Core.Model.CS;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactoryResolver = ICSharpCode.SharpDevelop.Dom.NRefactoryResolver.NRefactoryResolver;

namespace VVVV.HDE.CodeEditor.LanguageBindings.CS
{
    public class CSCompletionProvider : DefaultCompletionDataProvider
    {
        protected IDocumentLocator FDocumentLocator;
        
        public CSCompletionProvider(IDocumentLocator documentLocator)
        {
            FDocumentLocator = documentLocator;
        }
        
        public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            // We can return code-completion items like this:
            
            //return new ICompletionData[] {
            //	new DefaultCompletionData("Text", "Description", 1)
            //};
            
            var resultList = new List<ICompletionData>();
            
            var document = FDocumentLocator.GetVDocument(fileName) as CSDocument;
            if (document == null)
            	return resultList.ToArray();
            
            var parseInfo = document.ParseInfo;
            var projectContent = parseInfo.MostRecentCompilationUnit.ProjectContent;
            
            var resolver = new NRefactoryResolver(projectContent.Language);
            var expressionResult = FindExpression(parseInfo, textArea);
            
            Debug.WriteLine(string.Format("Generating completion data for expression result {0}", expressionResult));

            ArrayList completionData = null;
            if (charTyped == '.')
            {
                PreSelection = null;
                var rr = resolver.Resolve(expressionResult,
                                          parseInfo,
                                          textArea.MotherTextEditorControl.Text);
                
                if (rr != null)
                    completionData = rr.GetCompletionData(projectContent);
            }
            else
            {
                PreSelection = "";
                completionData = resolver.CtrlSpace(textArea.Caret.Line + 1,
                                                    textArea.Caret.Column + 1,
                                                    parseInfo,
                                                    textArea.Document.TextContent,
                                                    expressionResult.Context);
            }
            
            if (completionData != null)
                AddCompletionData(ref resultList, completionData, expressionResult.Context);
            
            return resultList.ToArray();
        }
        
        /// <summary>
        /// Find the expression the cursor is at.
        /// Also determines the context (using statement, "new"-expression etc.) the
        /// cursor is at.
        /// </summary>
        Dom.ExpressionResult FindExpression(Dom.ParseInformation parseInfo, TextArea textArea)
        {
            var document = textArea.Document;
            var finder = new Dom.CSharp.CSharpExpressionFinder(parseInfo);

            var expression = finder.FindExpression(document.GetText(0, textArea.Caret.Offset), textArea.Caret.Offset);
            if (expression.Region.IsEmpty) {
                expression.Region = new Dom.DomRegion(textArea.Caret.Line + 1, textArea.Caret.Column + 1);
            }
            return expression;
        }

        void AddCompletionData(ref List<ICompletionData> resultList, ArrayList completionData, Dom.ExpressionContext context)
        {
            // used to store the method names for grouping overloads
            Dictionary<string, CSCompletionData> nameDictionary = new Dictionary<string, CSCompletionData>();

            // Add the completion data as returned by SharpDevelop.Dom to the
            // list for the text editor
            foreach (object obj in completionData) {
                if (!context.ShowEntry(obj))
                    continue;
                
                if (obj is string) {
                    // namespace names are returned as string
                    resultList.Add(new DefaultCompletionData((string)obj, "namespace " + obj, 5));
                } else if (obj is Dom.IClass) {
                    Dom.IClass c = (Dom.IClass)obj;
                    resultList.Add(new CSCompletionData(c));
                } else if (obj is Dom.IMember) {
                    Dom.IMember m = (Dom.IMember)obj;
                    if (m is Dom.IMethod && ((m as Dom.IMethod).IsConstructor)) {
                        // Skip constructors
                        continue;
                    }
                    // Group results by name and add "(x Overloads)" to the
                    // description if there are multiple results with the same name.
                    
                    CSCompletionData data;
                    if (nameDictionary.TryGetValue(m.Name, out data)) {
                        data.AddOverload();
                    } else {
                        nameDictionary[m.Name] = data = new CSCompletionData(m);
                        resultList.Add(data);
                    }
                } else {
                    // Current ICSharpCode.SharpDevelop.Dom should never return anything else
                    throw new NotSupportedException();
                }
            }
        }
    }
}