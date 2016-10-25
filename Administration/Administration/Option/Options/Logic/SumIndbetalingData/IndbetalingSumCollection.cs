using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.Dynamics.Crm;

namespace Administration.Option.Options.Logic.SumIndbetalingData
{
	public class IndbetalingSumCollection
	{
		public List<IndbetalingSumPart> indbetalingParts = new List<IndbetalingSumPart>();
		public decimal Amount = 0;
		private Func<Indbetaling, Guid?> _getId;

		public CollectionTypeEnum CollectionType { get; private set; }
		public enum CollectionTypeEnum
		{
			byarbejde = 1,
			campaign = 2,
			indsamlingssted = 4,
			konto = 8,
		}

		public IndbetalingSumCollection(CollectionTypeEnum collectionType)
		{
			CollectionType = collectionType;
			SetGetId();
		}

		public void Add(Indbetaling indbetaling)
		{
			Guid? id = _getId(indbetaling);
			if (id.HasValue == false)
			{
				return;
			}

			decimal currentAmount = indbetaling.GetMoney().GetValueOrDefault(0);
			Amount += currentAmount;

			MakeSurePartsAreInList(id.Value, indbetaling.kilde);

			SumAmountToParts(id.Value, indbetaling.kilde, currentAmount);
		}

		private void SumAmountToParts(Guid id, Indbetaling.kildeEnum? kilde, decimal amount)
		{
			IndbetalingSumPart part = indbetalingParts.FirstOrDefault(lPart => lPart.Id == id && lPart.Kilde == kilde);

			part.Amount += amount;

			if (kilde.HasValue)
			{
				SumAmountToParts(id, null, amount);
			}
		}

		private void MakeSurePartsAreInList(Guid id, Indbetaling.kildeEnum? kilde)
		{
			IndbetalingSumPart part = indbetalingParts.FirstOrDefault(lPart => lPart.Id == id && lPart.Kilde == kilde);

			if (part == null)
			{
				part = new IndbetalingSumPart(id, kilde);
				indbetalingParts.Add(part);
			}

			if (kilde.HasValue)
			{
				MakeSurePartsAreInList(id, null);
			}
		}

		private void SetGetId()
		{
			switch (CollectionType)
			{
				case CollectionTypeEnum.byarbejde:
					_getId = (indbetaling => indbetaling.byarbejdeid);
					break;
				case CollectionTypeEnum.campaign:
					_getId = (indbetaling => indbetaling.campaignid);
					break;
				case CollectionTypeEnum.indsamlingssted:
					_getId = (indbetaling => indbetaling.indsamlingsstedid);
					break;
				case CollectionTypeEnum.konto:
					_getId = (indbetaling => indbetaling.kontoid);
					break;
				default:
					throw new Exception($"Unknown parttype {CollectionType}");
			}
		}
	}
}
