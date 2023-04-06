# RelatedItem
Used by various features to define different kinds of relations between items: for example, which item a character must have equipped to interact with some item in some way, which items can go inside a container, or which kind of item the target of a status effect must have for the effect to execute.


## Attributes

| Attribute            | Type       | Default value | Description                                                                                                                                                                                                                                                                    |
|----------------------|------------|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ExcludeBroken        | bool       | true          | Should broken (0 condition) items be excluded?                                                                                                                                                                                                                                 |
| RequireEmpty         | bool       | false         | Should only an empty inventory be considered valid? Can be used to, for example, make an item do something when there's nothing inside it.                                                                                                                                     |
| ExcludeFullCondition | bool       | false         | Should full condition (100%) items be excluded?                                                                                                                                                                                                                                |
| AllowVariants        | bool       | true          | Are item variants considered valid?                                                                                                                                                                                                                                            |
| Rotation             | float      | 0             | Only valid when used in the Containable definitions of an ItemContainer.<br/>Can be used to override the rotation of specific items in the container.                                                                                                                          |
| SetActive            | bool       | false         | Only valid when used in the Containable definitions of an ItemContainer.<br/>Can be used to force specific items to stay active inside the container (such as flashlights attached to a gun).                                                                                  |
| Msg                  | Identifier | ""            | Only valid for the RequiredItems of an ItemComponent. The localization tag of a message displayed if the required item isn't found (e.g. a notification about lack of ammo or fuel).                                                                                           |
| Optional             | bool       | false         | Only valid for the RequiredItems of an ItemComponent. Can be used to make the requirement optional,<br/>meaning that you don't need to have the item to interact with something, but having it may still affect what the interaction does (such as using a crowbar on a door). |
| IgnoreInEditor       | bool       | false         | Only valid for the RequiredItems of an ItemComponent. Can be used to ignore the requirement in the submarine editor,<br/>making it easier to for example make rewire things that require some special tool to rewire.                                                          |
| MatchOnEmpty         | bool       | false         | Should an empty inventory be considered valid? Can be used to, for example, make an item do something if there's a specific item, or nothing, inside it.                                                                                                                       |
| TargetSlot           | int        | -1            | Index of the slot the target must be in when targeting a Contained item                                                                                                                                                                                                        |
| Hide                 | bool       | false         | Only valid when used in the Containable definitions of an ItemContainer.<br/>Only affects when ItemContainer.hideItems is false. Doesn't override the value.                                                                                                                   |
| ItemPos              | Vector2?   | Zero          | Overrides the position defined in ItemContainer. Only valid when used in the Containable definitions of an ItemContainer.                                                                                                                                                      |

