﻿using System;
using System.Collections.Generic;
using System.Linq;
using RuntimeUnityEditor.Core.Inspector;
using RuntimeUnityEditor.Core.Utils;
using RuntimeUnityEditor.Core.Utils.Abstractions;
using UnityEngine;

#pragma warning disable CS1591

namespace RuntimeUnityEditor.Core.Clipboard
{
    /// <summary>
    /// Window that allows copying references to objects and using them later when invoking methods or setting fields/props.
    /// </summary>
    public class ClipboardWindow : Window<ClipboardWindow>
    {
        /// <summary>
        /// Contents of the clipboard.
        /// </summary>
        public static readonly List<object> Contents = new List<object>();
        private Vector2 _scrollPos;
        private string _editingIndex;
        private string _editingValue;

        protected override void Initialize(InitSettings initSettings)
        {
            Title = "Clipboard";
            MinimumSize = new Vector2(250, 100);
            DefaultScreenPosition = ScreenPartition.LeftUpper;
        }

        protected override void DrawContents()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

            if (Contents.Count == 0)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("You can copy objects to clipboard by clicking the 'C' button in inspector, or by running the 'copy(object)' command in REPL. Structs are copied by value, classes by reference.\n\n" +
                                    "Clipboard contents can be used in REPL by running the 'paste(index)' command, or in inspector when invoking a method.\n\n" +
                                    "Press 'X' to remove item from clipboard, right click on it to open a menu with more options.", IMGUIUtils.LayoutOptionsExpandWidthTrue);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
            }
            else
            {
                // Draw clipboard items
                GUILayout.BeginVertical();
                {
                    const int widthIndex = 35;
                    const int widthName = 70;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Index", GUILayout.Width(widthIndex), GUILayout.ExpandWidth(false));
                        GUILayout.Label("Type", GUILayout.Width(widthName), GUILayout.ExpandWidth(false));
                        GUILayout.Label("Value", IMGUIUtils.LayoutOptionsExpandWidthTrue);
                    }
                    GUILayout.EndHorizontal();

                    const string editControlNamePrefix = "clipboard_edit_";

                    for (var index = 0; index < Contents.Count; index++)
                    {
                        GUILayout.BeginHorizontal(GUI.skin.box);
                        {
                            var content = Contents[index];

                            if (GUILayout.Button(index.ToString(), GUI.skin.label, GUILayout.Width(widthIndex), GUILayout.ExpandWidth(false)) && IMGUIUtils.IsMouseRightClick())
                                ContextMenu.Instance.Show(content);

                            var type = content?.GetType();

                            if (GUILayout.Button(type?.Name ?? "NULL", GUI.skin.label, GUILayout.Width(widthName), GUILayout.ExpandWidth(false)) && IMGUIUtils.IsMouseRightClick())
                                ContextMenu.Instance.Show(content);

                            var prevEnabled = GUI.enabled;
                            GUI.enabled = type != null && typeof(IConvertible).IsAssignableFrom(type);

                            GUI.changed = false;
                            var controlName = editControlNamePrefix + index;
                            GUI.SetNextControlName(controlName);

                            var isBeingEdited = _editingIndex == controlName;
                            var prevColor = GUI.backgroundColor;
                            if (isBeingEdited) GUI.backgroundColor = _editingValue == null ? Color.green : Color.yellow;

                            var newVal = GUILayout.TextField(isBeingEdited && _editingValue != null ? _editingValue : ToStringConverter.ObjectToString(content), IMGUIUtils.LayoutOptionsExpandWidthTrue);

                            if (GUI.changed && type != null)
                            {
                                _editingIndex = controlName;
                                _editingValue = newVal;
                                try
                                {
                                    var converter = TomlTypeConverter.GetConverter(type);
                                    if (converter != null)
                                        Contents[index] = converter.ConvertToObject(newVal, type);
                                    else
                                        Contents[index] = Convert.ChangeType(newVal, type);

                                    _editingValue = null;
                                }
                                catch (Exception)
                                {
                                    //Console.WriteLine($"Could not convert string \"{newVal}\" to type \"{type.Name}\": {e.Message}");
                                }
                            }

                            GUI.enabled = prevEnabled;
                            GUI.backgroundColor = prevColor;

                            if (GUILayout.Button("X", IMGUIUtils.LayoutOptionsExpandWidthFalse))
                                Contents.RemoveAt(index);
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (_editingIndex != null && GUI.GetNameOfFocusedControl() != _editingIndex)
                    {
                        _editingIndex = null;
                        _editingValue = null;
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }

        public static IEnumerable<string> ResolveMethodParameters(IEnumerable<string> parameterStrings)
        {
            return parameterStrings.Select(ResolveString);
        }

        public static string ResolveString(string parameterString)
        {
            return TryExtractId(parameterString, out var id) ? Contents[id]?.ToString() ?? "null" : parameterString;
        }

        private static bool TryExtractId(string parameterString, out int id)
        {
            id = 0;
            return parameterString.Length >= 2 && parameterString.StartsWith("#") &&
                   int.TryParse(parameterString.Substring(1), out id) &&
                   Contents.Count > id && id >= 0;
        }

        public static bool TryGetObject(int id, out object obj)
        {
            if (id < 0 || id >= Contents.Count)
            {
                obj = null;
                return false;
            }

            obj = Contents[id];
            return true;
        }

        public static bool TryGetObject(string parameterString, out object obj)
        {
            if (!TryExtractId(parameterString, out var id))
            {
                obj = null;
                return false;
            }

            return TryGetObject(id, out obj);
        }
    }
}
