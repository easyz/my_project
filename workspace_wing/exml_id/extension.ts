import * as wing from 'wing';
var clipboard = require('copy-paste');
var fs = require('fs');
var path = require('path');

export function activate(context: wing.ExtensionContext) {
	console.log('Congratulations, your extension "HelloWing" is now active!');
	// wing.commands.registerCommand('extension.helloWing', helloWing);
	wing.commands.registerCommand("extension.copyExmlIds", copyExmlIds);
    // wing.commands.registerCommand("extension.copyexmlIds", copyexmlIds);
}

export function deactivate() {
}

// function helloWing() {
// 	wing.window.showInformationMessage('Hello Wing');
// }

function copyExmlIds(uri) {
    if (uri && uri.fsPath) {
        var curfileStr = fs.readFileSync(uri.fsPath, "utf-8");
        var myResult = findIds(curfileStr);
        if (myResult) {
            clipboard.copy(myResult, function () {
                wing.window.showInformationMessage("已成功解析exml中id,并复制对应的ts代码到剪切板,可使用粘贴操作到指定位置.");
            });
        }
        else {
            wing.window.showInformationMessage("exml中不存在id属性的节点.");
        }
        return;
    }
    var e = wing.window.activeTextEditor;
    if (!e) {
		wing.window.showWarningMessage("没有文件")
        return;
    }
    var fileName = e.document.fileName;
    var ext = path.extname(fileName);
    if (ext != '.exml') {
		wing.window.showWarningMessage("不是exml文件对象")
        return;
    }
    var content = e.document.getText();
    var result = findIds(content);
    if (result) {
		let tab = "    "
		let outResult = tab + "/////////////////////////////////////////////////////////////////////////////\n"
		outResult += tab + "// " + path.basename(fileName) + "\n"
		outResult += tab+ "/////////////////////////////////////////////////////////////////////////////\n"
		for (let re of result.split("\n")) {
			if (re) {
				outResult += tab + re + "\n"
			}
		}
		outResult += tab + "/////////////////////////////////////////////////////////////////////////////\n";
        clipboard.copy(outResult, function () {
            wing.window.showInformationMessage("已成功解析exml中id,并复制对应的ts代码到剪切板,可使用粘贴操作到指定位置.");
        });
    }
    else {
        wing.window.showInformationMessage("exml中不存在id属性的节点.");
    }
}

function findIds(text) {
    var lines = text.split(/[\r\n(\r\n)]/);
    var nss = findNameSpaces(lines.join(" "));
    var idexp = / id=\"(.*?)\"/ig;
    var result = ""
    var uimodule = is_eui(text) ? 'eui.' : 'egret.gui.';
    lines.forEach(function (line) {
        var temp = line.match(idexp);
        if (temp && temp.length > 0) {
            var classDefine = line.match(/<(.+?):(.+?) /);
            if (classDefine.length < 3) {
                return;
            }
            var classModule = void 0;
            if (classDefine[1] == "e") {
                classModule = uimodule;
            }
            else {
                classModule = nss[classDefine[1]];
                classModule = classModule.substring(0, classModule.length - 1);
            }
            var className = classDefine[2];
            var id = temp[0].replace(' id=', '').replace('"', '').replace('"', '');
            result += ("protected #1: " + classModule + "#2;").replace('#1', id).replace('#2', className) + '\n';
        }
    });
    return result;
}

function findNameSpaces(text) {
    var map = {};
    var names = text.match(/xmlns:(.+?)="(.+?)"/g);
    names.forEach(function (name) {
        var result = name.match(/xmlns:(.+?)="(.+?)"/);
        if (result.length == 3) {
            map[result[1]] = result[2];
        }
    });
    return map;
}
function is_eui(text) {
    if (text.indexOf('xmlns:e="http://ns.egret.com/eui"') > 0) {
        return true;
    }
    else {
        return false;
    }
}