﻿using System;
using System.Collections.Generic;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ChestsAnywhere.Components
{
    /// <summary>A UI which lets the player transfer items between a chest and their inventory.</summary>
    internal class ChestWithInventory : IClickableMenu
    {
        /*********
        ** Properties
        *********/
        /// <summary>The selected chest.</summary>
        protected ManagedChest SelectedChest;

        /// <summary>The selected chest's inventory.</summary>
        private List<Item> ChestItems => this.SelectedChest.Chest.items;

        /// <summary>The player inventory.</summary>
        private readonly List<Item> PlayerItems;

        /// <summary>The clickable components representing item slots.</summary>
        private readonly List<ClickableComponent> Slots = new List<ClickableComponent>();

        /// <summary>The item under the player's cursor.</summary>
        private Item HoveredItem;

        /// <summary>Whether the UI control is disabled, in which case it will no longer try to handle player interaction.</summary>
        protected bool IsDisabled { get; set; }

        /// <summary>The opacity at which to draw.</summary>
        protected float Opacity { get; set; } = 1;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ChestWithInventory()
        {
            this.width = Game1.tileSize * 13;
            this.height = Game1.tileSize * 7 + Game1.tileSize / 2;
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2 + Game1.tileSize;
            this.PlayerItems = Game1.player.Items;
            this.InitializeComponents();
        }

        /// <summary>The method invoked when the player's cursor on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        public override void performHoverAction(int x, int y)
        {
            if (this.IsDisabled)
                return;

            // find hovered item
            Item item = null;
            try
            {
                for (int i = this.Slots.Count - 1; i >= 0; i--)
                {
                    // chest
                    if (i < 36)
                    {
                        if (this.Slots[i].containsPoint(x, y))
                        {
                            if (i < this.ChestItems.Count && this.ChestItems[i] != null)
                            {
                                item = this.ChestItems[i];
                                break;
                            }
                        }
                    }

                    // player
                    else
                    {
                        if (this.Slots[i].containsPoint(x, y))
                        {
                            if (this.PlayerItems[i - 36] != null)
                            {
                                item = this.PlayerItems[i - 36];
                                break;
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                item = null; // happens when hovering over an unavailable slot (e.g. player doesn't have bigger backpack)
            }

            // set
            this.HoveredItem = item;
        }

        /// <summary>The method invoked when the player left-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.IsDisabled)
                return;

            for (int i = this.Slots.Count - 1; i >= 0; i--)
            {
                // chest to player
                if (i < 36)
                {
                    if (this.Slots[i].containsPoint(x, y))
                    {
                        if (i < this.ChestItems.Count && this.ChestItems[i] != null)
                        {
                            this.ChestItems[i] = this.TryAddItem(this.ChestItems[i], true);
                            if (this.ChestItems[i] == null)
                                this.ChestItems.RemoveAt(i);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }

                // player to chest
                else
                {
                    if (this.Slots[i].containsPoint(x, y))
                    {
                        if (this.PlayerItems[i - 36] != null)
                        {
                            this.PlayerItems[i - 36] = this.TryAddItem(this.PlayerItems[i - 36], false);
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }
            }
        }

        /// <summary>The method invoked when the player right-clicks on the inventory.</summary>
        /// <param name="x">The X-position of the cursor.</param>
        /// <param name="y">The Y-position of the cursor.</param>
        /// <param name="playSound">Whether to enable sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (this.IsDisabled)
                return;

            for (int i = this.Slots.Count - 1; i >= 0; i--)
            {
                // chest to player
                if (i < 36)
                {
                    if (this.Slots[i].containsPoint(x, y))
                    {
                        if (i < this.ChestItems.Count && this.ChestItems[i] != null)
                        {
                            Item itemToPlace = this.ChestItems[i].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.ChestItems[i].Stack > 1)
                            {
                                itemToPlace.Stack = this.ChestItems[i].Stack / 2;
                                itemToPlace = this.TryAddItem(itemToPlace, true);
                                if (itemToPlace == null)
                                    this.ChestItems[i].Stack -= this.ChestItems[i].Stack / 2;
                                if (this.ChestItems[i].Stack <= 0)
                                    this.ChestItems.RemoveAt(i);
                            }
                            else
                            {
                                itemToPlace = this.TryAddItem(itemToPlace, true);
                                if (itemToPlace == null && this.ChestItems[i].Stack == 1)
                                    this.ChestItems.RemoveAt(i);
                                else
                                    this.ChestItems[i].Stack--;
                            }
                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }

                // player to chest
                else
                {
                    if (this.Slots[i].containsPoint(x, y))
                    {
                        if (this.PlayerItems[i - 36] != null)
                        {
                            Item itemToPlace = this.PlayerItems[i - 36].getOne();
                            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && this.PlayerItems[i - 36].Stack > 1)
                            {
                                itemToPlace.Stack = this.PlayerItems[i - 36].Stack / 2;
                                itemToPlace = this.TryAddItem(itemToPlace, false);
                                if (itemToPlace == null)
                                    this.PlayerItems[i - 36].Stack -= this.PlayerItems[i - 36].Stack / 2;
                            }
                            else
                            {
                                itemToPlace = this.TryAddItem(itemToPlace, false);
                                if (itemToPlace == null && this.PlayerItems[i - 36].Stack == 1)
                                    this.PlayerItems[i - 36] = null;
                                else
                                    this.PlayerItems[i - 36].Stack--;
                            }

                            if (playSound)
                                Game1.playSound("coin");
                        }
                    }
                }
            }
        }

        /// <summary>Render the inventory UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public override void draw(SpriteBatch sprites)
        {
            // background
            sprites.DrawMenuBackground(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 16, this.yPositionOnScreen + Game1.tileSize / 16, this.width - Game1.tileSize / 8, this.height - Game1.tileSize / 8), bisect: true);

            // slots
            this.DrawChestSlots(sprites);
            this.DrawPlayerSlots(sprites);

            // items
            this.DrawChestItems(sprites);
            this.DrawPlayerItems(sprites);

            // tooltips
            if (this.HoveredItem != null)
            {
                string hoverText = this.HoveredItem.getHoverBoxText(this.HoveredItem);
                string hoverTitle = null;
                if (hoverText == null)
                {
                    hoverText = this.HoveredItem.getDescription();
                    hoverTitle = this.HoveredItem.Name;
                }

                IClickableMenu.drawToolTip(sprites, hoverText, hoverTitle, this.HoveredItem);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Trigger the chest item sort behaviour.</summary>
        public void SortChestItems()
        {
            ItemGrabMenu.organizeItemsInList(this.ChestItems);
            Game1.playSound("Ship");
        }

        /// <summary>Trigger the inventory item sort behaviour.</summary>
        public void SortInventory()
        {
            ItemGrabMenu.organizeItemsInList(this.PlayerItems);
            Game1.playSound("Ship");
        }

        /// <summary>Try to add an item to the player or chest inventory, and return the remaining stack.</summary>
        /// <param name="itemToPlace">The item to add.</param>
        /// <param name="toPlayer">Whether to add the item to the player inventory (otherwise, to the chest).</param>
        private Item TryAddItem(Item itemToPlace, bool toPlayer)
        {
            // to player
            if (toPlayer)
            {
                foreach (Item item in this.PlayerItems)
                {
                    if (item == null || !item.canStackWith(itemToPlace))
                        continue;

                    itemToPlace.Stack = item.addToStack(itemToPlace.Stack);
                    if (itemToPlace.Stack <= 0)
                        return null;
                }
                if (this.PlayerItems.Contains(null))
                {
                    for (int i = 0; i < this.PlayerItems.Count; i++)
                    {
                        if (this.PlayerItems[i] == null)
                        {
                            this.PlayerItems[i] = itemToPlace;
                            return null;
                        }
                    }
                }
                return itemToPlace;
            }

            // to chest
            foreach (Item item in this.ChestItems)
            {
                if (item == null || !item.canStackWith(itemToPlace))
                    continue;

                itemToPlace.Stack = item.addToStack(itemToPlace.Stack);
                if (itemToPlace.Stack <= 0)
                    return null;
            }
            if (this.ChestItems.Count < 36)
            {
                this.ChestItems.Add(itemToPlace);
                return null;
            }
            return itemToPlace;
        }

        /// <summary>Initialise the inventory for rendering.</summary>
        private void InitializeComponents()
        {
            // slots
            int wCount = 1;
            int y = this.yPositionOnScreen + Game1.tileSize / 2;
            while (wCount < 3)
            {
                int yCount = 0;
                while (yCount < 3)
                {
                    int xCount = 1;
                    int x = this.xPositionOnScreen + Game1.tileSize / 2;
                    while (xCount < 13)
                    {
                        this.Slots.Add(
                            new ClickableComponent(new Rectangle(x, y, Game1.tileSize, Game1.tileSize), wCount == 1 ? $"Chest Slot {yCount * 12 + xCount}" : $"Player Slot {yCount * 12 + xCount}")
                        );
                        x += Game1.tileSize;
                        xCount++;
                    }
                    y += Game1.tileSize;
                    yCount++;
                }
                y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
                wCount++;
            }
        }

        /// <summary>Render the player items.</summary>
        /// <param name="sprites">The sprites to render.</param>
        private void DrawPlayerItems(SpriteBatch sprites)
        {
            int slotCount = 36;

            while (slotCount < 72)
            {
                if (slotCount - 36 >= this.PlayerItems.Count)
                    break;

                this.PlayerItems[slotCount - 36]?.drawInMenu(sprites, new Vector2(this.Slots[slotCount].bounds.X, this.Slots[slotCount].bounds.Y), this.Slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f, this.Opacity, 1f);
                slotCount++;
            }
        }

        /// <summary>Render the chest items.</summary>
        /// <param name="sprites">The sprites to render.</param>
        private void DrawChestItems(SpriteBatch sprites)
        {
            int slotCount = 0;
            while (slotCount < 72)
            {
                if (slotCount >= this.ChestItems.Count)
                    break;

                this.ChestItems[slotCount]?.drawInMenu(sprites, new Vector2(this.Slots[slotCount].bounds.X, this.Slots[slotCount].bounds.Y), this.Slots[slotCount].containsPoint(Game1.getMouseX(), Game1.getMouseY()) ? 1f : 0.8f, this.Opacity, 1f);
                slotCount++;
            }
        }

        /// <summary>Render the player slots.</summary>
        /// <param name="sprites">The sprites to render.</param>
        private void DrawPlayerSlots(SpriteBatch sprites)
        {
            int yCount = 0;
            int y = this.yPositionOnScreen + this.height / 2 + Game1.tileSize / 4;
            while (yCount < 3)
            {
                int xCount = 1;
                int x = this.xPositionOnScreen + Game1.tileSize / 2;
                while (xCount < 13)
                {
                    Rectangle slot = yCount * 12 + xCount <= Game1.player.maxItems
                        ? Sprites.Menu.Slot
                        : Sprites.Menu.SlotDisabled;
                    sprites.Draw(Sprites.Menu.Sheet, slot, x, y, Game1.tileSize, Game1.tileSize, Color.White * this.Opacity);
                    x += Game1.tileSize;
                    xCount++;
                }
                y += Game1.tileSize;
                yCount++;
            }
        }

        /// <summary>Render the chest slots.</summary>
        /// <param name="sprites">The sprites to render.</param>
        private void DrawChestSlots(SpriteBatch sprites)
        {
            int yCount = 0;
            int y = this.yPositionOnScreen + Game1.tileSize / 2;
            while (yCount < 3)
            {
                int xCount = 0;
                int x = this.xPositionOnScreen + Game1.tileSize / 2;
                while (xCount < 12)
                {
                    sprites.Draw(Sprites.Menu.Sheet, Sprites.Menu.Slot, x, y, Game1.tileSize, Game1.tileSize, Color.White * this.Opacity);
                    x += Game1.tileSize;
                    xCount++;
                }
                y += Game1.tileSize;
                yCount++;
            }
        }
    }
}
