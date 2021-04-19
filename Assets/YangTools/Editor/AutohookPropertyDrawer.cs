/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-16 
*/
// NOTE put in a Editor folder
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[CustomPropertyDrawer(typeof(AutohookAttribute))]
public class AutohookPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.objectReferenceValue != null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        //不是RectTransform
        if (!(property.serializedObject.targetObject is RectTransform))
        {
            Debug.LogError("请挂载在RectTransform下(只能在UI下用特性--Autohook)");
            return;
        }

        AutohookAttribute autohookAttribute = (AutohookAttribute)attribute;
        if (autohookAttribute.hookType == AutohookAttribute.HookType.Component)
        {
            //property是单个序列化的属性
            Component component = FindAutohookTarget(property);
            if (component != null)
            {
                property.objectReferenceValue = component;
            }
            else
            {
                Debug.LogError($"{property.name}:没有找到对应组件--子节点路径:{autohookAttribute.relativePath}");
            }
        }
        else if (autohookAttribute.hookType == AutohookAttribute.HookType.Prefab)
        {
            string name = property.name;
            if (name.EndsWith("Prefab"))
            {
                name = name.Remove(name.Length - 6);
            }

            GameObject prefab = null;
            if (autohookAttribute.useDefault)
            {
                prefab = FindPrefab(property, name, autohookAttribute.prefabPath);
            }
            else
            {
                prefab = FindPrefab(property, autohookAttribute.prefabName, autohookAttribute.prefabPath);
            }

            if (prefab != null)
            {
                property.objectReferenceValue = prefab;
            }
            else
            {
                Debug.LogError($"{property.name}:没有找到对应预制物体--预制体路径:{autohookAttribute.prefabPath}");
            }
        }

        EditorGUI.PropertyField(position, property, label);

    }

    private GameObject FindPrefab(SerializedProperty property, string name, string assertPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { assertPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == prefabName)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        return null;
    }

    /// <summary>
    /// 获取SerializedProperty，并找到可以插入其中的本地组件。
    /// Local在这个上下文中意味着它是附加到同一个游戏对象的组件。
    /// 这可以很容易地更改为使用GetComponentInParent/GetComponentInChildren
    /// </summary>
    /// <param name="property">单个序列化的属性</param>
    private Component FindAutohookTarget(SerializedProperty property)
    {
        SerializedObject root = property.serializedObject;

        if (root.targetObject is Component)
        {
            // 获得这个属性的类型
            System.Type type = GetTypeFromProperty(property);

            if (type == null) return null;

            // 然后使用GetComponent(type)查看我们的对象上是否有一个组件
            Component component = (Component)root.targetObject;
            Component[] components = component.GetComponentsInChildren(type);
            foreach (var item in components)
            {
                AutohookAttribute autohookAttribute = (AutohookAttribute)attribute;

                //默认物体根据名字搜索
                if (autohookAttribute.useDefault)
                {
                    //确保GameObject不要有重名的
                    if (item.gameObject.name == property.name)
                    {
                        return item.gameObject.GetComponent(type);
                    }
                }
                else
                {
                    #region 获得路径
                    string resultPath = "";

                    Transform tempNode = item.gameObject.transform;
                    string nodePath = "/" + tempNode.name;

                    //遍历到顶，求得路径
                    while (tempNode != Selection.activeGameObject.transform)
                    {
                        //取得上级
                        tempNode = tempNode.parent;
                        //求出/在哪
                        int index = nodePath.IndexOf('/');
                        //把得到的路径插入
                        nodePath = nodePath.Insert(index, "/" + tempNode.name);
                    }

                    //去掉开头斜杠
                    if (nodePath.StartsWith("/"))
                    {
                        nodePath = nodePath.Remove(0, 1);
                    }

                    //最终路径
                    resultPath = nodePath;
                    #endregion

                    if (resultPath == autohookAttribute.relativePath)
                    {
                        return item.gameObject.GetComponent(type);
                    }
                }
            }
        }
        else
        {
            Debug.Log("FindAutohookTarget is Error");
        }

        return null;
    }

    /// <summary>
    /// 使用反射从序列化属性获取类型
    /// </summary>
    /// <param name="property"></param>
    private static System.Type GetTypeFromProperty(SerializedProperty property)
    {
        // 首先，让我们获取这个序列化属性所属组件的类型。
        System.Type rootType = property.serializedObject.targetObject.GetType();

        //const BindingFlags InstanceBindFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        //…然后，使用反射得到这个SerializedProperty表示的属性的原始字段信息…
        System.Reflection.FieldInfo fieldInfo = rootType.GetField(property.propertyPath);

        if (fieldInfo == null)
        {
            return null;
        }

        // …使用它我们可以返回原始的.net类型!
        return fieldInfo.FieldType;
    }

}