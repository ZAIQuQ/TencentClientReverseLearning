using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonoPlugin
{
    /// <summary>
    ///  对私有成员、属性、字段的操作
    /// </summary>
    public static class OperaterPrivate
    {
        // 1. 获取私有字段
        public static T GetPrivateField<T>(this object instance, string fieldname)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flag);
            return (T)field.GetValue(instance);
        }

        // 2. 获取私有属性
        public static T GetPrivateProperty<T>(this object instance, string propertyname)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            PropertyInfo field = type.GetProperty(propertyname, flag);
            return (T)field.GetValue(instance, null);
        }

        // 3. 设置私有成员
        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flag);
            field.SetValue(instance, value);
        }

        // 4. 设置私有属性
        public static void SetPrivateProperty(this object instance, string propertyname, object value)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            PropertyInfo field = type.GetProperty(propertyname, flag);
            field.SetValue(instance, value, null);
        }

        // 5. 调用私有方法
        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(name, flag);
            return (T)method.Invoke(instance, param);
        }
    }

    public class Cheat : UnityEngine.MonoBehaviour
    {
        private void OnGUI()
        {
            UnityEngine.GUI.Label(new UnityEngine.Rect(10, 10, 200, 100), "Q 分数+114514\nW 飞行速度减半\nE 飞行速度翻倍\nR 修改碰撞");
        }

        public void FixedUpdate()
        {
            // Q 键修改分数
            if(Input.GetKeyDown(KeyCode.Q))
            {
                var birdscript = UnityEngine.GameObject.FindWithTag("player").GetComponent<BirdScripts>();
                var gamecontroller = UnityEngine.GameObject.FindWithTag("player").GetComponent<GamePlayController>();

                if (birdscript != null) 
                {
                    birdscript.score += 114514;

                    // 这里需要调用GamePlayController的实例来执行setScore方法，刷新显示的分数
                    gamecontroller.setScore(birdscript.score);
                }
            }

            // W 键修改前进速度(反射机制)
            if (Input.GetKeyDown(KeyCode.W))
            {
                var birdscript = UnityEngine.GameObject.FindWithTag("player").GetComponent<BirdScripts>();
                Type type = birdscript.GetType();
                var forwardSpeed = OperaterPrivate.GetPrivateField<float>(birdscript, "forwardSpeed");
                Debug.Log("forwardSpeed:" + forwardSpeed);
                forwardSpeed /= 2;
                OperaterPrivate.SetPrivateField(birdscript, "forwardSpeed", forwardSpeed);
            }

            // E 键修改前进速度(反射机制)
            if (Input.GetKeyDown(KeyCode.E))
            {
                var birdscript = UnityEngine.GameObject.FindWithTag("player").GetComponent<BirdScripts>();
                Type type = birdscript.GetType();
                var forwardSpeed = OperaterPrivate.GetPrivateField<float>(birdscript, "forwardSpeed");
                Debug.Log("forwardSpeed:" + forwardSpeed);
                forwardSpeed *= 2;
                OperaterPrivate.SetPrivateField(birdscript, "forwardSpeed", forwardSpeed);
            }

            // R 切换是否无碰撞
            if (Input.GetKeyDown(KeyCode.R))
            {
                var player = UnityEngine.GameObject.FindWithTag("Player");
                var birdscript = player.GetComponent<BirdScripts>();
                // 切换刚体碰撞逻辑的触发开关状态
                player.GetComponent<Collider2D>().isTrigger = !player.GetComponent<Collider2D>().isTrigger;
            }
        }
    }
}
