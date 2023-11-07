import frida
import sys

# 不可阻挡
invincible_js = """
Java.perform(() => {
    
    var libil2cpp = Process.findModuleByName("libil2cpp.so");
    
    // OnTriggerEnter2D
    var offset1 = 0x5E2BCC;
    var addr1 = libil2cpp.base.add(offset1);
    var arr1 = [0xE5, 0x00, 0x00, 0xEA]; 
   
    Memory.protect(addr1, 0x1000, 'rwx');
    Memory.writeByteArray(addr1, arr1);
    
    // OnCollisionEnter2D
    var offset2 = 0x5E3158;
    var addr2 = libil2cpp.base.add(offset2);
    var arr2 = [0x1C, 0x00, 0x00, 0xEA]; 

    Memory.protect(addr2, 0x1000, 'rwx');
    Memory.writeByteArray(addr2, arr2);
});
"""


# 额外加分
add_score_js = """
Java.perform(() => {
    
    var libil2cpp = Process.findModuleByName("libil2cpp.so");
    // UpdateScore
    var offset1 = 0x5E0A0C;
    var addr1 = libil2cpp.base.add(offset1);
    var arr1 = [0x64, 0x10, 0x81, 0xE2]; // 0x64加100分 0x5E0A0C
    
    Memory.protect(addr1, 0x1000, 'rwx');
    Memory.writeByteArray(addr1, arr1);
});
"""


def on_message(message, data):
    if message['type'] == 'send':
        print("[*] {0}".format(message['payload']))
    else:
        print(message)

def main():
    device = frida.get_usb_device()
    app = device.get_frontmost_application()
    process = device.attach(app.pid)

    while (True):
        jscode = ""
        print("Hook成功!菜单如下:")
        print("[0] : 关闭插件")
        print("[1] : 开启无敌")
        print("[2] : 额外加分")
        choice = int(input(">> "))
        match choice:
            case 0:
                break
            case 1:
                jscode = invincible_js
                print("已经不可阻挡")
            case 2:
                jscode = add_score_js
                print("开启额外加分")
            case _:
                print("error code")
        try:        
            script = process.create_script(jscode)
            script.on('message', on_message)
            script.load()
        except:
            pass
        
    process.detach()
if __name__ == "__main__":
    main()