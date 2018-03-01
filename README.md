# Windows Registry Redirection
[![GitHub license](https://img.shields.io/github/license/peitaosu/Win-Reg-Redirect.svg)](https://github.com/peitaosu/Win-Reg-Redirect/blob/master/LICENSE)

This project is supposed to redirect all registry calls of process to virtual registry.

## Requirements
* EasyHook (WinRegRedirector.exe)
* python 2.x (Reg2JSON.py)

## Supported APIs
* RegOpenKey(Ex)
* RegCreateKey(Ex)
* RegDeleteKey(Ex)
* RegSetValue(Ex)
* RegQueryValue(Ex)
* RegCloseKey

## V_REG.json Sample
```
{
    "Keys": {
        "hkey_local_machine": {
            "Keys": {
                "software":{
                    "Keys": {
                        "microsoft": {
                            "Keys": {},
                            "Values": []
                        }
                    },
                    "Values": []
                }
            },
            "Values": [
                {
                    "Name": "value_name",
                    "Type": "REG_DWORD",
                    "Data": "0x00000001"
                }
            ]
        }
    }
}
```

## Usage

Please put `V_REG.json` in the same location as WinRegRedirector.exe.

```
WinRegRedirector.exe ProcessID
                     ProcessName.exe
                     PathToExecutable

#example

> WinRegRedirector.exe 1234
> WinRegRedirector.exe notepad.exe
> WinRegRedirector.exe C:\Windows\notepad.exe
```

## Convert V_REG.json from .reg file

```
> python Tool\Reg2JSON.py in.reg out.json
```