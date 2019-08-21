import 'package:flutter/material.dart';
import 'package:viia_sample/models/transaction.dart';

class TransactionCell {
  static ListTile _createTransactionTile(Transaction transaction) {
    return ListTile(
        contentPadding: EdgeInsets.symmetric(horizontal: 20.0, vertical: 10.0),
        leading: Container(
          padding: EdgeInsets.only(right: 12.0, top: 8.0),
          child: Icon(Icons.payment, color: Colors.black12),
        ),
        title: Text(
          transaction.text,
          overflow: TextOverflow.ellipsis,
          style: TextStyle(color: Colors.black87, fontWeight: FontWeight.bold),
        ),
        subtitle: Text(
          transaction.date,
          style: TextStyle(color: Colors.black26, fontWeight: FontWeight.w200),
        ),
        trailing: Container(
            child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: <Widget>[
            Text(
              transaction.amount.toString(),
              style: TextStyle(color: Colors.red),
            ),
            Text(transaction.balance.toString())
          ],
        )));
  }

  static Widget build(Transaction transaction) {
    return _createTransactionTile(transaction);
  }
}
