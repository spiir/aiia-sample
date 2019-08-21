import 'package:flutter/material.dart';
import 'package:viia_sample/models/account.dart';

import 'common_widget_styles.dart';

class AccountDetailsWidget {
  static PreferredSize build(Account account) {
    return PreferredSize(
        preferredSize: const Size.fromHeight(50.0),
        child: _buildBottom(account));
  }

  static Widget _buildBottom(Account account) {
    return Container(
        height: 55.0,
        padding: EdgeInsets.only(top: 10.0, left: 10.0),
        child: Container(
            child: Padding(
          padding: new EdgeInsets.symmetric(horizontal: .0, vertical: .0),
          child: Column(
            children: <Widget>[
              Row(
                children: <Widget>[
                  Text(account.number,
                      style: CommonWidgetStyles.appBarTextStyle())
                ],
              ),
              Row(
                children: <Widget>[
                  Text('Owned by ',
                      style: CommonWidgetStyles.appBarTextStyle()),
                  Text(account.owner,
                      style: CommonWidgetStyles.appBarTextStyle())
                ],
              ),
            ],
          ),
        )));
  }
}
