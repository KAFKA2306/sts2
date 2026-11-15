# Slay the Spire 2 新キャラMOD

移動式の舞台を背負う人形遣い「からくり座の座長」を制作する。初期レリック「三幕仕掛け」で、毎ターンの3枚目に出すカードの種類が人形の動きを決める。

- [初版の仕様とSteam版での合格条件](https://github.com/KAFKA2306/sts2/issues/1)
- [リポジトリの管理ルール](AGENTS.md)
- [配布物と導入手順](https://github.com/KAFKA2306/sts2/releases)

## 制作を始める

リポジトリのルートで `git submodule update --init` を実行する。[桜井スキルの上流リポジトリ](https://github.com/kociden/sakurai-skill) の固定版が `.agents/skills/sakurai-skill` に入る。

ビルドの出発点はAlchyrの新キャラテンプレート、BaseLib、MegaDot。初版のカードと実機の確認手順はIssue #1に記載している。 [テンプレートのセットアップ](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)
