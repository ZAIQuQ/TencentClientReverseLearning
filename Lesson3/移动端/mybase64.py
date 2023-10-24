import base64
# 标准索引
standard_charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
# 自定义索引
my_charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/"
# 构建转换
trans = str.maketrans(standard_charset, my_charset)
# 要解码的数据
encoded_data = "DMD2vxKYDLvezuriqND2DhP3BJfdtuWWrxe9pq=="
for i in range(2):
    # 先进行编码转换
    encoded_data = encoded_data.translate(trans)
    print(encoded_data)
    # 再进行正常base64编码
    data = base64.b64decode(encoded_data.encode()).decode()
    print(data)
    encoded_data = data