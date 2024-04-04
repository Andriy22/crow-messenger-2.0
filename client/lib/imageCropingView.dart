import 'package:crop_image/crop_image.dart';
import 'package:flutter/material.dart';

class ImageCropingView extends StatefulWidget {
  Image image;
  void Function(CropController) onCropFinished;

  ImageCropingView(this.image, this.onCropFinished);

  @override
  State<ImageCropingView> createState() => _ImageCropingViewState();
}

class _ImageCropingViewState extends State<ImageCropingView> {
  var croppedControler = CropController(aspectRatio: 1);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text("Croping image"),),
      body: CropImage(controller: croppedControler,
        image: widget.image,
        onCrop: (rect) {

        },
      ),
      bottomNavigationBar: Row(
        children: [
          IconButton(
            icon: Icon(Icons.check),
            onPressed: () {
              Navigator.pop(context);
              widget.onCropFinished(croppedControler);
            },
          )
        ],
      ),
    );
  }

}

