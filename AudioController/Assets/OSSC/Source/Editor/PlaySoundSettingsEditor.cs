using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OSSC;
using OSSC.Model;

[CustomPropertyDrawer(typeof(PlaySoundSettings))]
[CanEditMultipleObjects]
public class PlaySoundSettingsEditor : PropertyDrawer {

    private float textHeight;
    private Color labelColor = new Color(1, 0.686f, 0.011f);

    private SoundSelection selected;

    private SerializedProperty rootProperty;
    private SerializedProperty database;
    private SerializedProperty categoryId;
    private SerializedProperty soundItemId;
    private SerializedProperty parent;
    private SerializedProperty fadeInTime;
    private SerializedProperty fadeOutTime;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) * (property.isExpanded ? 9 : 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        textHeight = GUI.skin.textField.lineHeight + GUI.skin.textField.margin.top + GUI.skin.textField.border.top;

        label = EditorGUI.BeginProperty(position, label, property);

        position.height = 16;

        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
        if (property.isExpanded) {
            rootProperty = property;
            database = property.FindPropertyRelative("database");
            categoryId = property.FindPropertyRelative("categoryId");
            soundItemId = property.FindPropertyRelative("soundItemId");
            parent = property.FindPropertyRelative("parent");
            fadeInTime = property.FindPropertyRelative("fadeInTime");
            fadeOutTime = property.FindPropertyRelative("fadeOutTime");

            GenericMenu menu = BuildMenu(database.objectReferenceValue as SoundControllerData, categoryId, soundItemId);

            Rect rect = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel++;

            rect.y += textHeight;
            float prevX = rect.x;
            rect.x += 16;
            var buttonText = new GUIContent("Select sound");
            float prevWidth = rect.width;
            rect.width = GUI.skin.button.CalcSize(buttonText).x;
            if (EditorGUI.DropdownButton(rect, buttonText, FocusType.Keyboard)) {
                menu.ShowAsContext();
            }
            rect.width = prevWidth;

            //rect.y += textHeight;
            rect.x += GUI.skin.button.CalcSize(buttonText).x;
            rect.width -= rect.x - 16;
            EditorGUI.BeginDisabledGroup(true);
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = labelColor;
            string path = BuildSelectedSoundPath();
            EditorGUI.TextField(rect, BuildSelectedSoundPath());
            GUI.backgroundColor = prevColor;
            EditorGUI.EndDisabledGroup();
            EditorGUI.LabelField(rect, new GUIContent("", path));
            rect.x = prevX;
            rect.width = prevWidth;

            rect.y += textHeight;
            EditorGUI.PropertyField(rect, database, new GUIContent(database.displayName));

            rect.y += textHeight;
            EditorGUI.PropertyField(rect, parent, new GUIContent(parent.displayName));

            rect.y += textHeight;
            EditorGUI.PropertyField(rect, fadeInTime, new GUIContent(fadeInTime.displayName));

            rect.y += textHeight;
            EditorGUI.PropertyField(rect, fadeOutTime, new GUIContent(fadeOutTime.displayName));

            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private GenericMenu BuildMenu(SoundControllerData database, SerializedProperty categoryId, SerializedProperty soundItemId) {
        GenericMenu menu = new GenericMenu();
        if (database == null) {
            menu.AddDisabledItem(new GUIContent("Select a Database"));
        } else {
            foreach (CategoryItem category in database.items) {
                if (category.soundItems != null) {
                    foreach (SoundItem soundItem in category.soundItems) {
                        SoundSelection ss = new SoundSelection {
                            category = category,
                            soundItem = soundItem
                        };
                        menu.AddItem(new GUIContent(ss.Path()), ss.Path() == selected.Path(), OnSoundSelected, ss);
                    }
                }
            }
        }
        return menu;
    }

    private void OnSoundSelected(object selection) {
        SoundSelection ss = (SoundSelection)selection;
        selected = ss;
        categoryId.stringValue = ss.category.id;
        soundItemId.stringValue = ss.soundItem.id;
        rootProperty.serializedObject.ApplyModifiedProperties();
    }

    private string BuildSelectedSoundPath() {
        string path = "";

        if (database != null) {
            SoundControllerData db = database.objectReferenceValue as SoundControllerData;
            CategoryItem category = null;
            SoundItem soundItem = null;
            if (db != null && db.items != null) {
                category = Array.Find(db.items, (item) => item.id == categoryId.stringValue);
                if (category != null && category.soundItems != null) {
                    soundItem = Array.Find(category.soundItems, (item) => item.id == soundItemId.stringValue);
                }
            }
            if (soundItem != null) {
                path = category.name + "/" + soundItem.name;
            }
        }

        return path;
    }

    private struct SoundSelection {
        public CategoryItem category;
        public SoundItem soundItem;

        public string Path() {
            return Equals(default(SoundSelection)) ? "" : category.name + "/" + soundItem.name;
        }
    }
}
