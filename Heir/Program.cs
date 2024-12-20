﻿using Heir;
using Heir.Runtime;

var program = new HeirProgram();
var testFile = SourceFile.FromPath("./Test.heir", isMainFile: true);
//Console.WriteLine(testFile.Tokenize().ToString());
//Console.WriteLine();
var syntaxTree = testFile.Parse();
syntaxTree.Display();
//Console.WriteLine();
//testFile.Bind().GetBoundNode(syntaxTree).Display();
//Console.WriteLine();
//Console.WriteLine(testFile.GenerateBytecode().ToString());
//Console.WriteLine();
program.LoadFile(testFile);

var result = program.Evaluate();
var resultString = result is ObjectValue objectValue ? objectValue.ToString() : result ?? "none";
Console.WriteLine(resultString); // TODO: some sort of repr function
