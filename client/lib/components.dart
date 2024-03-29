import 'package:flutter/material.dart';

Card CardButton(String text, Color color, void Function() onTap) {
  return Card(
    color: color,
    child: InkWell(
        splashColor: Colors.blue[300],
        onTap: onTap,
        child: Padding(padding: EdgeInsets.fromLTRB(10, 10, 10, 10),
            child: Text(text))),);
}