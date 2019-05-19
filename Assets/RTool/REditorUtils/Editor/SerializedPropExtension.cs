﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace REditor
{
	internal static class SerializedPropExtension
	{
		#region Simple string path based extensions
		public static string ParentPath(this SerializedProperty prop)
		{
			int lastDot = prop.propertyPath.LastIndexOf('.');
			if (lastDot == -1) // No parent property
				return "";

			return prop.propertyPath.Substring(0, lastDot);
		}
		public static SerializedProperty GetParentProp(this SerializedProperty prop)
		{
			string parentPath = prop.ParentPath();
			return prop.serializedObject.FindProperty(parentPath);
		}
		#endregion
    
		public static void ExpandHierarchy(this SerializedProperty prop, bool expand = true)
		{
			prop.isExpanded = expand;
			SerializedProperty parent = GetParentProp(prop);
			if (parent != null)
				ExpandHierarchy(parent);
		}
		#region Reflection based extensions
		public static object GetValue<T>(this SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}
			if (obj is T)
				return (T) obj;
			return null;
		}

		public static Type GetTypeReflection(this SerializedProperty prop)
		{
			object obj = GetParent<object>(prop);
			if (obj == null)
				return null;
			
			Type objType = obj.GetType();
			const BindingFlags bindingFlags = System.Reflection.BindingFlags.GetField
											  | System.Reflection.BindingFlags.GetProperty
											  | System.Reflection.BindingFlags.Instance
											  | System.Reflection.BindingFlags.NonPublic
											  | System.Reflection.BindingFlags.Public;
			FieldInfo field = objType.GetField(prop.name, bindingFlags);
			if (field == null)
				return null;
			return field.FieldType;
		}

		public static T GetParent<T>(this SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements.Take(elements.Length - 1))
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}
			return (T) obj;
		}

		private static object GetValue(object source, string name)
		{
			if (source == null)
				return null;
			Type type = source.GetType();
			FieldInfo f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f == null)
			{
				PropertyInfo p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null)
					return null;
				return p.GetValue(source, null);
			}
			return f.GetValue(source);
		}

		private static object GetValue(object source, string name, int index)
		{
			var enumerable = GetValue(source, name) as IEnumerable;
			if (enumerable == null)
				return null;
			var enm = enumerable.GetEnumerator();
			while (index-- >= 0)
				enm.MoveNext();
			return enm.Current;
		}
		public static bool HasAttribute<T>(this SerializedProperty prop)
		{
			object[] attributes = GetAttributes<T>(prop);
			if (attributes != null)
			{
				return attributes.Length > 0;
			}
			return false;
		}
		public static object[] GetAttributes<T>(this SerializedProperty prop)
		{
			object obj = GetParent<object>(prop);
			if (obj == null)
				return new object[0];

			Type attrType = typeof (T);
			Type objType = obj.GetType();
			const BindingFlags bindingFlags = System.Reflection.BindingFlags.GetField
			                                  | System.Reflection.BindingFlags.GetProperty
			                                  | System.Reflection.BindingFlags.Instance
			                                  | System.Reflection.BindingFlags.NonPublic
			                                  | System.Reflection.BindingFlags.Public;
			FieldInfo field = objType.GetField(prop.name, bindingFlags);
			if (field != null)
				return field.GetCustomAttributes(attrType, true);
			return new object[0];
		}
		public static SerializedProperty[] FindPropsOfType<T>(this SerializedObject obj, bool enterChildren = false)
		{
			List<SerializedProperty> foundProps = new List<SerializedProperty>();
			Type propType = typeof(T);

			var iterProp = obj.GetIterator();
			iterProp.Next(true);

			if (iterProp.NextVisible(enterChildren))
			{
				do
				{
					var propValue = iterProp.GetValue<T>();
					if (propValue == null)
					{
						if (iterProp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (iterProp.objectReferenceValue != null && iterProp.objectReferenceValue.GetType() == propType)
								foundProps.Add(iterProp.Copy());
						}
					}
					else
					{
						foundProps.Add(iterProp.Copy());
					}
				} while (iterProp.NextVisible(enterChildren));
			}
            return foundProps.ToArray();
		}

		#endregion
	}
}