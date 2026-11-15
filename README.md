# Slay the Spire 2 新キャラMOD

移動式の舞台を背負う人形遣い「からくり座の座長」を制作する。初期レリック「三幕仕掛け」で、毎ターンの3枚目に出すカードの種類が人形の動きを決める。

- [初版の仕様とSteam版での合格条件](https://github.com/KAFKA2306/sts2/issues/1)
- [リポジトリの管理ルール](AGENTS.md)
- [配布物と導入手順](https://github.com/KAFKA2306/sts2/releases)

## 制作を始める

リポジトリのルートで `git submodule update --init` を実行する。[桜井スキルの上流リポジトリ](https://github.com/kociden/sakurai-skill) の固定版が `.agents/skills/sakurai-skill` に入る。

ビルドの出発点はAlchyrの新キャラテンプレート、BaseLib、MegaDot。初版のカードと実機の確認手順はIssue #1に記載している。 [テンプレートのセットアップ](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)

## 開発ビルド

`mod/KarakuriZa` にキャラクター、14枚のカード、初期レリック、日英テキストを置く。戦闘・休憩所の座長イラストと14枚のカード絵を収録する。戦闘の動きはGodotのAnimationPlayerで再生する。

```sh
Sts2DataDir=/abs/path/to/game/data \
MEGADOT=/abs/path/to/megadot-mono \
./mod/KarakuriZa/build.sh
```

`.NET SDK 10.0.404` と [MegaDot 4.5.1-m.14](https://megadot.megacrit.com/api/4.5.1-m.14/) を使う。依存は [Alchyrテンプレート 2.5.3](https://www.nuget.org/packages/Alchyr.Sts2.Templates/2.5.3) と [BaseLib 3.4.8](https://www.nuget.org/packages/Alchyr.Sts2.BaseLib/3.4.8) を基準にする。ビルドは `mod/KarakuriZa/dist/KarakuriZa.zip` とハッシュを出す。コンパイルだけなら `--compile-only` を付ける。

[Steam実測の期待値と採点方法](eval/README.md) を参照。公開参照アセンブリでのコンパイルと梱包を確認済み。対応Steam BuildIDと戦闘の実測は未確認。
