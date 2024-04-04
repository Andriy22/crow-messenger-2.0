import 'package:client/auth.dart';
import 'package:client/data.dart';
import 'package:client/loading.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import 'chatsView.dart';
import 'loginView.dart';

void main() async {
  runApp(const CrowMessenger());
}

class CrowMessenger extends StatelessWidget {
  const CrowMessenger({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Crow Messenger',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.orange),
        useMaterial3: true,
      ),
      home: LoadingView()
    );
  }
}