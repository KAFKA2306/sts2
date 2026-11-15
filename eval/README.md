# Steam実測

[Issue #1](https://github.com/KAFKA2306/sts2/issues/1) の初期状態と行動列に、比較する値を `expected.json` で対応づける。照明のブロック差は選択先2、もう一方1で9点を配分する。連撃のケースでは初撃で片方を倒し、次の9点が生存側に全て入るかを確認する。

Steamから読み取った値を同じキーで実測JSONに渡す。全項目を採取できたケースが再生済み、全項目が一致したケースが合格になる。

```sh
./eval/score.sh /path/to/observed.json
```

[操作MODのAPI](https://github.com/barrel10/STS2-Agent-mod/blob/829ba230e79839ea26dcb9749bf93f268adff697/docs/api.md) で `/health`、`/state`、`/actions/available`、`/action` を順に使い、行動後に安定した状態を採取する。同じ盤面から `/vision/screenshot` を採取する。固定盤面のセットアップと二灯照明の第2対象を自動操作する経路は未接続。

キャラクター選択から出発までの操作は `scenarios/character-select.json` に固定している。対象PCの操作MOD 0.16.2を起動し、オートプレイを止め、検証用プロファイルをキャラ選択画面に置く。

```sh
./eval/replay.sh eval/scenarios/character-select.json /tmp/karakuriza-embark
```

初期デッキと三幕仕掛けのIDを照合する。各ステップに直前・直後の盤面、合法手、リクエスト、応答、指定場面のJPEGを保存する。エラー後は盤面を確認してから再開する。初戦勝利と報酬を含む実測ケースの後半は、固定シードの行動列をSteam版で採取して追加する。
