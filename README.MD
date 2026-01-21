# 🎮 Pitu HvH - Essentials (v2.0.0)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-v342+-green)](https://github.com/roflmuffin/CounterStrikeSharp)
[![CS2](https://img.shields.io/badge/CS2-Plugin-blue)](https://store.steampowered.com/app/730/CounterStrike_2/)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

> 🇧🇷 **Plugin essencial para servidores CS2, totalmente traduzido para Português!**

O único plugin que você precisa para rodar um servidor HvH de sucesso. Inclui recursos básicos como **reset de score** e **rage quit**, além de restrições opcionais para **armas**, **fogo amigo**, **double tap** e proteções contra exploits/crashes.

### ⚠️ Se você usa este plugin, NÃO precisa mais do [RapidFireFix](https://github.com/HvH-gg/RapidFireFix).

---

## ✨ Funcionalidades

- ✅ Suporte a Custom Votes (requer [CS2-CustomVotes](https://github.com/imi-tat0r/CS2-CustomVotes))
- ✅ Reset de score com `!rs`
- ✅ Rage quit com `!rq`
- ✅ Restrição de armas (AWP, Scout, Auto Sniper) **por time**
- ✅ Fogo amigo apenas de utilitários (granadas, molotov)
- ✅ Proteção contra Double Tap (bloquear, refletir dano, refletir sem matar)
- ✅ Proteção contra Teleporte/Airstuck (também previne crash do servidor)
- ✅ Funciona em **todos os modos de jogo** (Casual, Competitivo, Mata-Mata)
- ✅ Detecta troca de time e remove arma se novo time está no limite
- ✅ Comandos funcionam no chat E no console (`rs`, `RS`, `rq`, `RQ`)

---

## 📋 Requisitos

- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
- [CounterStrikeSharp (v342+)](https://github.com/roflmuffin/CounterStrikeSharp/releases)

---

## 📦 Instalação

1. Instale o Metamod:Source e CounterStrikeSharp
2. Baixe a [última release](../../releases) do plugin
3. Extraia a pasta `addons` no diretório `game/csgo/` do seu servidor
4. Edite o arquivo de configuração conforme necessário:
   - Localizado em: `addons/counterstrikesharp/configs/plugins/CS2-Essentials/CS2-Essentials.json`
5. Reinicie o servidor

---

## ⚙️ Configuração

Para recarregar a configuração após editar, use `hvh_cfg_reload` no console do servidor.

```json
{
  "RapidFireFixMethod": 1,
  "RapidFireReflectScale": 1,
  "AllowedAwpCount": -1,
  "AllowedScoutCount": -1,
  "AllowedAutoSniperCount": -1,
  "UnmatchedFriendlyFire": true,
  "RestrictTeleport": true,
  "AllowAdPrint": false,
  "AllowSettingsPrint": true,
  "AllowResetScore": true,
  "AllowRageQuit": true,
  "ChatPrefix": "{Gold} ⋆ {Blue}Pitu{Gold}HvH{Gold} ⋆",
  "CustomVoteSettings": {
    "FriendlyFireVote": false,
    "TeleportFixVote": false,
    "RapidFireVote": "full",
    "Style": "center"
  },
  "ConfigVersion": 4
}
```

### Explicação das Opções

| Opção | Descrição | Valores |
|-------|-----------|---------|
| `RapidFireFixMethod` | Proteção contra Double Tap | `0` permite, `1` bloqueia, `2` reflete dano, `3` reflete sem matar |
| `RapidFireReflectScale` | Porcentagem do dano refletido | `0.0` a `1.0` |
| `AllowedAwpCount` | AWPs permitidas por time | `-1` ilimitado, `0` proibido, `1+` limite |
| `AllowedScoutCount` | Scouts permitidas por time | `-1` ilimitado, `0` proibido, `1+` limite |
| `AllowedAutoSniperCount` | Auto Snipers por time | `-1` ilimitado, `0` proibido, `1+` limite |
| `UnmatchedFriendlyFire` | Fogo amigo só de utilitários | `true` ou `false` |
| `RestrictTeleport` | Bloqueia exploit de teleporte | `true` ou `false` |
| `AllowSettingsPrint` | Mostra configurações ao spawnar | `true` ou `false` |
| `AllowResetScore` | Permite comando `!rs` | `true` ou `false` |
| `AllowRageQuit` | Permite comando `!rq` | `true` ou `false` |
| `ChatPrefix` | Prefixo das mensagens no chat | String com cores |

---

## 💬 Comandos

### Para Jogadores

| Comando | Descrição |
|---------|-----------|
| `!rs` ou `!resetscore` | Reseta seu score e estatísticas |
| `!rq` ou `!ragequit` | Sai do servidor com estilo |
| `!settings` | Mostra as configurações do servidor |

> 💡 **Dica:** Os comandos também funcionam no console sem o `!`:
> - `rs`, `RS`, `Rs`, `rS`, `resetscore`, `RESETSCORE`
> - `rq`, `RQ`, `Rq`, `rQ`, `ragequit`, `RAGEQUIT`

---

## 🖥️ ConVars (Console do Servidor)

Você pode alterar configurações em tempo real usando ConVars. **Essas mudanças NÃO são salvas** e serão resetadas ao reiniciar o servidor.

| ConVar | Descrição |
|--------|-----------|
| `hvh_restrict_rapidfire <0-3>` | 0=permitir, 1=bloquear, 2=refletir, 3=refletir sem matar |
| `hvh_rapidfire_reflect_scale <0-1>` | Porcentagem do dano refletido |
| `hvh_restrict_teleport <0/1>` | Bloqueia exploit de teleporte/airstuck |
| `hvh_restrict_awp <n>` | AWPs por time (-1=ilimitado, 0=proibido) |
| `hvh_restrict_scout <n>` | Scouts por time (-1=ilimitado, 0=proibido) |
| `hvh_restrict_auto <n>` | Auto Snipers por time (-1=ilimitado, 0=proibido) |
| `hvh_unmatched_friendlyfire <0/1>` | Fogo amigo só de utilitários |
| `hvh_resetscore <0/1>` | Ativa/desativa comando !rs |
| `hvh_ragequit <0/1>` | Ativa/desativa comando !rq |
| `hvh_cfg_reload` | Recarrega configuração do arquivo |

---

## 🎨 Cores do ChatPrefix

Você pode usar todas as cores disponíveis no CounterStrikeSharp no prefixo do chat:

`Default`, `White`, `Red`, `LightRed`, `DarkRed`, `BlueGrey`, `Green`, `Lime`, `Olive`, `LightGreen`, `Blue`, `LightBlue`, `Purple`, `Magenta`, `Grey`, `LightGrey`, `Yellow`, `Gold`, `Orange`, `Silver`

**Exemplo:**
```json
"ChatPrefix": " {Gold} ⋆ {Blue}Pitu{Gold}HvH{Gold} ⋆"
```

---

## 🗳️ Custom Votes (Opcional)

Se você tiver o [CS2-CustomVotes](https://github.com/imi-tat0r/CS2-CustomVotes) instalado, pode habilitar votações:

| Comando | Configuração |
|---------|--------------|
| `!rapidfire` | `CustomVoteSettings.RapidFireVote` (`off`, `simple`, `full`) |
| `!friendlyfire` | `CustomVoteSettings.FriendlyFireVote` (`true`/`false`) |
| `!teleport` | `CustomVoteSettings.TeleportFixVote` (`true`/`false`) |

> ⚠️ **Aviso:** A restrição de teleporte previne o exploit de crash do servidor. Se você habilitar a votação de teleporte, o servidor ficará vulnerável ao exploit.

---

## 🛡️ Detalhes das Funcionalidades

### Restrição de Armas
- Define limites de AWP, Scout e Auto Sniper **por time**
- Quando um jogador tenta comprar além do limite:
  - Arma é removida automaticamente
  - Dinheiro é reembolsado
  - Aviso é exibido no chat
- Se um jogador troca de time com uma arma restrita e o novo time já está no limite, a arma é removida
- Jogadores podem pegar sua própria arma dropada (não conta como nova)

### Fogo Amigo de Utilitários
Quando `UnmatchedFriendlyFire: true`:
- ❌ Tiros em aliados são **ignorados**
- ✅ Granadas, Molotovs e HE afetam aliados normalmente

### Proteção contra Double Tap
| Modo | Comportamento |
|------|---------------|
| `0` - Permitir | Double tap funciona normalmente |
| `1` - Bloquear | Dano de double tap é bloqueado |
| `2` - Refletir | Dano é refletido para o atacante |
| `3` - Refletir (Seguro) | Dano é refletido mas não mata o atacante |

---

## 🤝 Créditos

- **Pitu** - Modificações, traduções PT-BR, melhorias v2.0
- **HvH.gg / imi-tat0r** - Plugin original
- [Metamod:Source](https://www.sourcemm.net/)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

---

## 📄 Licença

Este projeto é licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 🐛 Bugs e Sugestões

Encontrou um bug ou tem uma sugestão? Abra uma [issue](../../issues)!

---

<p align="center">
  <b>Feito para a comunidade de HvH BR de CS2</b>
</p>
