

import 'package:flutter/material.dart';

class BaseCellCard {
  static Widget build(Widget child) {
    return Card(
      elevation: 2.0,
      margin: new EdgeInsets.symmetric(horizontal: 10.0, vertical: 6.0),
      child: Container(
        decoration: BoxDecoration(color: Colors.white12),
        child: child,
      ),
    );
  }
}