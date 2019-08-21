import 'package:flutter/material.dart';
import 'package:viia_sample/models/account.dart';

class AccountCell {
  static ListTile _buildAccountTile(
      Account account, void Function() onAccountTap) {
    return ListTile(
      title: Text(account.name),
      subtitle: Text(account.number),
      leading: Container(
        padding: EdgeInsets.only(left: 8.0, top: 6.0),
        child: Icon(Icons.account_balance, color: Colors.black12),
      ),
      trailing: Container(
        child: Icon(Icons.chevron_right, color: Colors.black45),
      ),
      onTap: onAccountTap,
    );
  }

  static Widget build(Account account, void Function() onAccountTap) {
    return _buildAccountTile(account, onAccountTap);
  }
}
