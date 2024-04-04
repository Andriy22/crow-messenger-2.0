import 'package:flutter/material.dart';

Card CardButton(String text, Color color, void Function() onTap, {Widget? icon = null}) {
  var textBox = Row(children: [
    Text(text)
  ],);

  if(icon != null) {
    textBox.children.insert(0, Padding(padding: EdgeInsets.fromLTRB(0, 0, 4, 0), child: icon,));
  }

  return Card(
    color: color,
    child: InkWell(
        splashColor: Colors.blue[300],
        onTap: onTap,
        child: Padding(padding: EdgeInsets.fromLTRB(12, 8, 12, 8),
            child: textBox)),);
}