import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

import 'consts.dart';

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

Widget GridImages(List<String> imageLinks, BuildContext context) {
  Image image = Image.network(imageLinks[0]);
  //var isHorizontal = image.width > image.height;
  var maxWidth = BoxConstraints(maxWidth: MediaQuery.of(context).size.width - 100).maxWidth/2-9;
  Image getImage(String imageLink, double width, double height) {
    return width < 0
        ? Image(
          image: NetworkImage("${URL}/static/users/${imageLink}"))
        : Image(
            image: NetworkImage("${URL}/static/users/${imageLink}"),
            width: maxWidth * width,
            height: maxWidth * height,
            fit: BoxFit.cover);
  }

  //Widget getFirstContainer()

  return switch(imageLinks.length) {
    1 => getImage(imageLinks[0], -1, 2),
    2 => Column(children: [
      getImage(imageLinks[0], 2, 1),
      getImage(imageLinks[1], 2, 1),
    ],),
    3 => Column(children: [
        getImage(imageLinks[0], 2, 1),
        Row(children: [
          getImage(imageLinks[1], 1, 1),
          getImage(imageLinks[2], 1, 1)
        ],)
      ],),
    4 => Column(children: [
      Row(children: [
        getImage(imageLinks[0], 1, 1),
        getImage(imageLinks[1], 1, 1)
      ],),
      Row(children: [
        getImage(imageLinks[2], 1, 1),
        getImage(imageLinks[3], 1, 1)
      ],)
    ],),
    // TODO: Handle this case.
    _ => getImage(imageLinks[0], 2, 2),
  };
}
