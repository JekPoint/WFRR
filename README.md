# Windows File/Registry Redirection
[![GitHub license](https://img.shields.io/github/license/peitaosu/Win-FS-Reg-Redirect.svg)](https://github.com/peitaosu/Win-FS-Reg-Redirect/blob/master/LICENSE)

This project is supposed to redirect all file/registry calls of process to virtual file system/registry.

## Requirements
- WinFSRegRedirector.exe
   * EasyHook 
   * Newtonsoft.Json
   ```
   nuget restore WinFSRegRedirector.sln
   ```
- Reg2JSON.py
   * python 2.x

## Supported APIs
* RegOpenKey(Ex)
* RegCreateKey(Ex)
* RegDeleteKey(Ex)
* RegSetValue(Ex)
* RegQueryValue(Ex)
* RegCloseKey
* CreateFileW
* DeleteFileW
* CopyFileW

## V_REG.json Sample
* Keys: please use the key name with lower case.
* Values: support REG_DWORD, REG_QWORD, REG_SZ and REG_BINARY types.
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

## V_FS.json Sample
* Source: source directory path.
* Destination: target directory path which you want to redirect to.
```
{
    "Mapping": [
        {
            "Source": "",
            "Destination": ""
        },
        {
            "Source": "",
            "Destination": ""
        }
    ]
}
```


## Usage

Please put `V_REG.json` and `V_FS.json` in the same location as WinFSRegRedirector.exe.

```
WinFSRegRedirector.exe ProcessID
                     ProcessName.exe
                     PathToExecutable

#example

> WinFSRegRedirector.exe 1234
> WinFSRegRedirector.exe notepad.exe
> WinFSRegRedirector.exe C:\Windows\notepad.exe
```

## Convert V_REG.json from .reg file

```
> python Tool\Reg2JSON.py in.reg out.json

# suppose the in.reg is using utf-16, if not, please change the encoding in get_reg_str_list()
```