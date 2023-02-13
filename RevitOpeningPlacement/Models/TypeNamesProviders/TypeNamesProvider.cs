﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.TypeNamesProviders {
    internal class TypeNamesProvider : ITypeNamesProvider {
        private readonly bool _mepSystemIsRound;

        public TypeNamesProvider(bool mepSystemIsRound) {
            _mepSystemIsRound = mepSystemIsRound;
        }

        public IEnumerable<string> GetTypeNames() {
            if(_mepSystemIsRound) {
                yield return RevitRepository.TypeName[OpeningType.WallRound];
            }
            yield return RevitRepository.TypeName[OpeningType.WallRectangle];
        }
    }
}