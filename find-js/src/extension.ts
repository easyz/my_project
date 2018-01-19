'use strict';
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
let path = require("path")

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {

    // Use the console to output diagnostic information (console.log) and errors (console.error)
    // This line of code will only be executed once when your extension is activated
    // console.log('Congratulations, your extension "find-js" is now active!');

    // The command has been defined in the package.json file
    // Now provide the implementation of the command with  registerCommand
    // The commandId parameter must match the command field in package.json
    // let disposable = vscode.commands.registerCommand('extension.sayHello', () => {
    //     // The code you place here will be executed every time your command is executed

    //     // Display a message box to the user
    //     vscode.window.showInformationMessage('Hello World!');
    // });

    // context.subscriptions.push(disposable);

	context.subscriptions.push(vscode.commands.registerCommand("extension.openJsFile", openJsFile))
	
}

// this method is called when your extension is deactivated
export function deactivate() {
}

function openJsFile() {
	
	let e = vscode.window.activeTextEditor
    if (!e) {
        return
	}
	if (!vscode.workspace.rootPath) {
		return
	}
    let fileName = e.document.fileName
    let ext = path.extname(fileName)
    if (ext != ".ts") {
        return
	}
	vscode.commands.executeCommand("vscode.open", vscode.Uri.file(fileName.replace("src", "bin-debug").replace(".ts", ".js")))
}