﻿using CardRPG.Entities.Users;
using CardRPG.UseCases;
using Core.Collections;
using Core.Unity.Popups;
using UnityEngine;

namespace CardRPG.UI.Gameplay
{
    public class PlayerActionController : MonoBehaviour
    {
        private string _playerId;
        private Card[] _playerCards;

        private string _enemyId;
        private Card[] _enemyCards;

        private Entities.Gameplay.Card _lastSelectedCard;
        private bool _isLastSelectedCardEnemy;

        public void Init(
            string playerId,
            Card[] playerCards,
            string enemyId,
            Card[] enemyCards)
        {
            _playerId = playerId;
            _playerCards = playerCards;

            _enemyId = enemyId;
            _enemyCards = enemyCards;

            AssignOnCardSelected();
        }

        private async void OnCardSelected(Entities.Gameplay.Card card, bool isEnemy)
        {
            if (isEnemy && !_isLastSelectedCardEnemy)
            {
                await new AttackCommandHandler().Handle(
                    new AttackCommand(_playerId, _lastSelectedCard.Id.Value, _enemyId, card.Id.Value));

                var dto = await new GetGameStateQueryHandler().Handle(new GetGameStateQuery());
                GameObject.FindAnyObjectByType<Board>().Rebuild(dto);

                GameObject.FindAnyObjectByType<MessagesController>().Show($"Player attacked");
            }

            _lastSelectedCard = card;
            _isLastSelectedCardEnemy = isEnemy;
        }

        private void AssignOnCardSelected()
        {
            AssignOnCardSelected(_playerCards);
            AssignOnCardSelected(_enemyCards);
        }

        private void RemoveOnCardSelected()
        {
            RemoveOnCardSelected(_playerCards);
            RemoveOnCardSelected(_enemyCards);
        }

        private void AssignOnCardSelected(Card[] cards) =>
            cards.ForEach(card => card.OnCardSelected += OnCardSelected);

        private void RemoveOnCardSelected(Card[] cards) =>
           cards.ForEach(card => card.OnCardSelected -= OnCardSelected);

        private void OnDestroy()
        {
            RemoveOnCardSelected();
        }
    }
}
